// <copyright file="DocumentService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Threading.Tasks;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei;
using Cps.Fct.Hk.Ui.Interfaces.Model;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Model;
using Microsoft.Extensions.Logging;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Cps.Fct.Hk.Ui.ServiceClient.Ddei.Utils;
using Azure.Storage.Blobs;
using System.IO;

/// <summary>
/// Provides services for retrieving documents related to a case.
/// </summary>
public class DocumentService(
    ILogger<DocumentService> logger,
    IDdeiServiceClient apiClient,
    IConversionService conversionService,
    BlobServiceClient blobServiceClient)
    : IDocumentService
{
    private readonly ILogger<DocumentService> logger = logger;
    private readonly IDdeiServiceClient apiClient = apiClient;
    private readonly IConversionService conversionService = conversionService;
    private readonly BlobServiceClient? blobServiceClient = blobServiceClient;

    /// <inheritdoc/>
    public async Task<FileStreamResult?> GetMaterialDocumentAsync(string caseId, string link, CmsAuthValues cmsAuthValues, bool firstPageOnly = true)
    {
        string? tmpFileDownloadName = null;
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Downloading document with file path [{link}] ...");

            string encodedFilePath = this.UrlEncodeFilePath(link);

            var request = new GetDocumentRequest(caseId, encodedFilePath, Guid.NewGuid());

            FileStreamResult? downloadedDocument = await this.apiClient.GetMaterialDocumentAsync(request, cmsAuthValues).ConfigureAwait(false);

            if (downloadedDocument == null)
            {
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} No case material document found for file path [{link}]");
            }

            // Get document MIME type and use Aspose to generate a PDF
            string? contentType = FileUtils.GetMimeType(link);

            if (contentType != null)
            {
                bool tmpSaved;

                // Save the document to temporary storage (Blob storage in this case)
                tmpSaved = await this.conversionService.SaveDownloadedDocumentToTemporaryStorageAsync(downloadedDocument).ConfigureAwait(true);

                if (tmpSaved)
                {
                    string? blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName");

                    tmpFileDownloadName = downloadedDocument.FileDownloadName;
                    string fileDownloadName = $"{tmpFileDownloadName}.pdf";

                    // Convert the saved document to a PDF and store it in Blob Storage
                    string? pdfBlobUrl = await this.conversionService.ConvertToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly).ConfigureAwait(true);

                    if (pdfBlobUrl == null)
                    {
                        throw new Exception($"{LoggingConstants.HskUiLogPrefix} Error creating PDF document from [{tmpFileDownloadName}]");
                    }

                    if (this.blobServiceClient != null && !string.IsNullOrEmpty(blobContainerName))
                    {
                        BlobClient blobClient = this.blobServiceClient
                            .GetBlobContainerClient(blobContainerName)
                            .GetBlobClient($"preview_{tmpFileDownloadName}.pdf");

                        var pdfStream = new MemoryStream();
                        await blobClient.DownloadToAsync(pdfStream).ConfigureAwait(false);
                        pdfStream.Position = 0; // Reset stream position for reading

                        return new FileStreamResult(pdfStream, "application/pdf")
                        {
                            FileDownloadName = fileDownloadName,
                        };
                    }
                    else
                    {
                        this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} BlobServiceClient or BlobContainerName is null. Cannot proceed with Blob operations.");
                        throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} BlobServiceClient or BlobContainerName is not properly initialized.");
                    }
                }
            }

            return default;
        }
        catch (NotSupportedException ex)
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Error: {ex.Message} for file path [{link}]");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while downloading document with file path [{link}]");
            this.logger.LogError(ex, ex.Message);
            throw;
        }
        finally
        {
            // Delete the temporary files from Blob Storage
            if (!string.IsNullOrEmpty(tmpFileDownloadName) && this.blobServiceClient != null)
            {
                string? blobContainerName = Environment.GetEnvironmentVariable("BlobContainerName");

                if (!string.IsNullOrEmpty(blobContainerName))
                {
                    await this.DeleteBlobAsync(blobContainerName, $"tmp_{tmpFileDownloadName}", "Temporary file").ConfigureAwait(false);
                    await this.DeleteBlobAsync(blobContainerName, $"preview_{tmpFileDownloadName}.pdf", "Temporary preview or full document").ConfigureAwait(false);
                }
            }
        }
    }

    /// <summary>
    /// Deletes a blob from Azure Blob Storage and logs the result.
    /// </summary>
    /// <param name="blobContainerName">The name of the blob container where the blob is stored.</param>
    /// <param name="blobName">The name of the blob to be deleted.</param>
    /// <param name="description">A description of the blob (used in log messages).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the blobServiceClient is not initialized.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the blob container name is null or empty.</exception>
    internal async Task DeleteBlobAsync(string blobContainerName, string blobName, string description)
    {
        try
        {
            // Ensure blobServiceClient is not null
            if (this.blobServiceClient == null)
            {
                throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} BlobServiceClient is not initialized.");
            }

            // Ensure blobContainerName is not null or empty
            if (string.IsNullOrEmpty(blobContainerName))
            {
                throw new ArgumentNullException(nameof(blobContainerName), $"{LoggingConstants.HskUiLogPrefix} Blob container name cannot be null or empty.");
            }

            BlobClient blobClient = this.blobServiceClient
                .GetBlobContainerClient(blobContainerName)
                .GetBlobClient(blobName);

            // Delete the blob
            await blobClient.DeleteIfExistsAsync().ConfigureAwait(false);

            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} {description} [{blobName}] deleted from Blob Storage.");
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error deleting {description} [{blobName}] from Blob Storage.");
        }
    }

    /// <summary>
    /// URL-encodes the first forward slash ('/') in the given file path string while leaving the rest of the path unencoded.
    /// </summary>
    /// <param name="filePath">The file path to be encoded.</param>
    /// <returns>
    /// A string where the first forward slash ('/') is double-encoded as '%252F', and the rest of the file path remains unchanged.
    /// If no forward slash is found, the entire file path is returned URL-encoded.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the input <paramref name="filePath"/> is null, empty, or whitespace.</exception>
    internal string UrlEncodeFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException($"{LoggingConstants.HskUiLogPrefix} File path cannot be null or empty", nameof(filePath));
        }

        // Manually encode the first '/'
        // In this URL, the encoded first part %2F (which represents /) has been double-encoded as %252F.
        // To double-encode it, you need to encode % itself, which is %25, so %2F becomes %252F
        // The rest of the URL remains unchanged unless you need to double-encode other characters.
        string encodedFirstPart = "%252F";

        return encodedFirstPart + filePath;
    }
}
