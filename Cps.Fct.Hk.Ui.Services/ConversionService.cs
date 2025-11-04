// <copyright file="ConversionService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Aspose.Email.Mapi;
using Aspose.Pdf.Facades;
using Azure.Storage.Blobs;
using Common.Configuration;
using Common.Constants;
using Cps.Fct.Hk.Ui.Interfaces;
using DdeiClient.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Image = Aspose.Imaging.Image;

/// <summary>
/// Provides services for converting documents related to a case.
/// </summary>
public class ConversionService(ILogger<ConversionService> logger,
    BlobServiceClient? blobServiceClient,
    IConfiguration configuration)
    : IConversionService
{
    private readonly ILogger<ConversionService> logger = logger;
    private readonly BlobServiceClient? blobServiceClient = blobServiceClient;
    private readonly IConfiguration configuration = configuration;

    /// <inheritdoc />
    public async Task<bool> SaveDownloadedDocumentToTemporaryStorageAsync(FileStreamResult downloadedDocument)
    {
        string? blobContainerName = this.configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty;

        if (string.IsNullOrEmpty(blobContainerName))
        {
            this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} Blob container name is not set in the environment variables.");
            return false;
        }

        bool saved = false;
        if (this.blobServiceClient != null)
        {
            // Generate a unique temporary file name
            string temporaryFileName = $"tmp_{downloadedDocument.FileDownloadName}";

            // Get the container client
            BlobContainerClient containerClient = this.blobServiceClient.GetBlobContainerClient(blobContainerName);

            // Ensure the container exists (create if it does not)
            await containerClient.CreateIfNotExistsAsync().ConfigureAwait(true);

            // Get a reference to the blob (file) with the temporary name
            BlobClient blobClient = containerClient.GetBlobClient(temporaryFileName);

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    if (downloadedDocument.FileStream.CanRead == false)
                    {
                        this.logger.LogError($"{LoggingConstants.HskUiLogPrefix} The FileStream is empty. Cannot upload to Azure Blob Storage.");
                        return false;
                    }

                    await downloadedDocument.FileStream.CopyToAsync(memoryStream).ConfigureAwait(false);
                    memoryStream.Position = 0;  // Reset stream position

                    await blobClient.UploadAsync(memoryStream, overwrite: true).ConfigureAwait(false);
                }

                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Downloaded document saved temporarily to Azure Blob Storage as [{temporaryFileName}]");
                saved = true;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error occurred while saving the document to Azure Blob Storage [{temporaryFileName}].");
            }
        }

        return saved;
    }

    /// <inheritdoc />
    public async Task<string?> ConvertToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true)
    {
        string? pdfFilePath = null;

        try
        {
            // Determine the content type of the file
            string? contentType = FileUtils.GetMimeType(tmpFileDownloadName);

            if (contentType != null)
            {
                pdfFilePath = contentType switch
                {
                    var type when this.IsRasterImage(type) => await this.ConvertWithLoggingAsync(() => this.ConvertRasterImageToPdfDocumentAsync(tmpFileDownloadName), "Raster Image").ConfigureAwait(true),
                    "application/msword" => await this.ConvertWithLoggingAsync(() => this.ConvertDocToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "DOC").ConfigureAwait(true), // DOC and DOT files
                    "application/octet-stream" => await this.ConvertWithLoggingAsync(() => this.ConvertHtmlToPdfDocumentAsync(tmpFileDownloadName), "HTML").ConfigureAwait(true), // HTE, HTM and HTML files
                    "application/pdf" => await this.ConvertWithLoggingAsync(() => this.ConvertPdfToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "PDF").ConfigureAwait(true),
                    "application/rtf" => await this.ConvertWithLoggingAsync(() => this.ConvertDocToPdfDocumentAsync(tmpFileDownloadName), "RTF").ConfigureAwait(true),
                    "application/vnd.ms-excel" => await this.ConvertWithLoggingAsync(() => this.ConvertXlsToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "XLS").ConfigureAwait(true),
                    "application/vnd.ms-excel.sheet.macroEnabled.12" => await this.ConvertWithLoggingAsync(() => this.ConvertXlsToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "XLSM").ConfigureAwait(true),
                    "application/vnd.ms-word.document.macroEnabled.12" => await this.ConvertWithLoggingAsync(() => this.ConvertDocToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "DOCM").ConfigureAwait(true),
                    "application/vnd.ms-word.template.macroEnabled.12" => await this.ConvertWithLoggingAsync(() => this.ConvertDocToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "DOTM").ConfigureAwait(true),
                    "application/vnd.ms-outlook" => await this.ConvertWithLoggingAsync(() => this.ConvertMsgToPdfDocumentAsync(tmpFileDownloadName), "MSG").ConfigureAwait(true),
                    "application/vnd.ms-powerpoint" => await this.ConvertWithLoggingAsync(() => this.ConvertPptToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "PPT").ConfigureAwait(true),
                    "application/vnd.openxmlformats-officedocument.presentationml.presentation" => await this.ConvertWithLoggingAsync(() => this.ConvertPptToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "PPTX").ConfigureAwait(true),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.document" => await this.ConvertWithLoggingAsync(() => this.ConvertDocToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "DOCX").ConfigureAwait(true),
                    "application/vnd.openxmlformats-officedocument.wordprocessingml.template" => await this.ConvertWithLoggingAsync(() => this.ConvertDocToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "DOTX").ConfigureAwait(true),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" => await this.ConvertWithLoggingAsync(() => this.ConvertXlsToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "XLSX").ConfigureAwait(true),
                    "application/xml" => await this.ConvertWithLoggingAsync(() => this.ConvertXmlToPdfDocumentAsync(tmpFileDownloadName), "XML").ConfigureAwait(true),
                    "text/csv" => await this.ConvertWithLoggingAsync(() => this.ConvertXlsToPdfDocumentAsync(tmpFileDownloadName, firstPageOnly), "CSV").ConfigureAwait(true),
                    "text/plain" => await this.ConvertWithLoggingAsync(() => this.ConvertTxtToPdfDocumentAsync(tmpFileDownloadName), "TXT").ConfigureAwait(true),
                    "text/xml" => await this.ConvertWithLoggingAsync(() => this.ConvertXmlToPdfDocumentAsync(tmpFileDownloadName), "XML").ConfigureAwait(true),
                    _ => throw new NotSupportedException($"Unsupported content type: {contentType}")
                };
            }
        }
        catch (NotSupportedException notSupportedEx)
        {
            this.logger.LogError(notSupportedEx, $"{LoggingConstants.HskUiLogPrefix} Unsupported content type error for file: {tmpFileDownloadName}");
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting to PDF document.");
        }

        return pdfFilePath;
    }

    /// <inheritdoc />
    public async Task<string?> ConvertRasterImageToPdfDocumentAsync(string tmpFileDownloadName)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // PDF blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (HTML file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the image blob as a stream
                using (var imageStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(imageStream).ConfigureAwait(false);
                    imageStream.Position = 0;  // Reset stream position

                    // Load the image from the downloaded stream
                    using (Image image = Image.Load(imageStream))
                    {
                        // Save the image as a PDF to a new memory stream
                        using (var pdfStream = new MemoryStream())
                        {
                            image.Save(pdfStream, new Aspose.Imaging.ImageOptions.PdfOptions() { PdfDocumentInfo = new Aspose.Imaging.FileFormats.Pdf.PdfDocumentInfo() });

                            // Reset stream position before uploading to blob
                            pdfStream.Position = 0;

                            // Get a reference to the PDF blob (output file)
                            BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                            // Upload the PDF to Blob Storage
                            await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                            // Get the URI of the uploaded PDF file
                            pdfFileUrl = pdfBlobClient.Uri.ToString();

                            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted raster image to PDF and saved as [{pdfFileName}] in Blob Storage.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting Raster Image to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl; // Return the URL of the uploaded PDF
    }

    /// <inheritdoc />
    public async Task<string?> ConvertPdfToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // PDF output blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (HTML file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the original PDF blob as a stream
                using (var originalPdfStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(originalPdfStream).ConfigureAwait(false);
                    originalPdfStream.Position = 0;  // Reset stream position

                    var document = new Aspose.Pdf.Document(originalPdfStream);

                    // Create PdfFileEditor object for extracting pages
                    PdfFileEditor pdfEditor = new PdfFileEditor();

                    // Memory stream to hold the extracted pages
                    using (var extractedPdfStream = new MemoryStream())
                    {
                        // Pages to extract
                        int[] pagesToExtract;

                        if (firstPageOnly)
                        {
                            // Extract only the first page
                            pagesToExtract = [1];
                        }
                        else
                        {
                            // Extract all pages
                            pagesToExtract = Enumerable.Range(1, document.Pages.Count).ToArray();
                        }

                        // Extract the first page and save it to the extractedPdfStream
                        pdfEditor.Extract(originalPdfStream, pagesToExtract, extractedPdfStream);

                        // Reset the stream position before uploading
                        extractedPdfStream.Position = 0;

                        // Get a reference to the new PDF blob (output file)
                        BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                        // Upload the extracted PDF to Blob Storage
                        await pdfBlobClient.UploadAsync(extractedPdfStream, overwrite: true).ConfigureAwait(false);

                        // Get the URI of the uploaded PDF file
                        pdfFileUrl = pdfBlobClient.Uri.ToString();

                        string pageCount = firstPageOnly
                            ? "1"
                            : document.Pages.Count.ToString(CultureInfo.InvariantCulture);

                        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Extracted {pageCount} page(s) from PDF and saved as [{pdfFileName}] in Blob Storage.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error extracting pages from PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl;
    }

    /// <inheritdoc />
    public async Task<string?> ConvertTxtToPdfDocumentAsync(string tmpFileDownloadName)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // Output PDF blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (HTML file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the original TXT blob as a stream
                using (var txtStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(txtStream).ConfigureAwait(false);
                    txtStream.Position = 0;  // Reset stream position

                    // Read the content of the TXT file from the memory stream
                    string txtContent;
                    using (var reader = new StreamReader(txtStream))
                    {
                        txtContent = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }

                    // Create a new PDF document
                    var pdfDocument = new Aspose.Pdf.Document();
                    Aspose.Pdf.Page pdfPage = pdfDocument.Pages.Add();

                    // Add the TXT content to the PDF
                    var textFragment = new Aspose.Pdf.Text.TextFragment(txtContent);
                    pdfPage.Paragraphs.Add(textFragment);

                    // Memory stream to hold the converted PDF
                    using (var pdfStream = new MemoryStream())
                    {
                        // Save the PDF to the memory stream
                        pdfDocument.Save(pdfStream);

                        // Reset the stream position before uploading to Blob storage
                        pdfStream.Position = 0;

                        // Get a reference to the new PDF blob (output file)
                        BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                        // Upload the PDF to Blob Storage
                        await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                        // Get the URI of the uploaded PDF file
                        pdfFileUrl = pdfBlobClient.Uri.ToString();

                        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted TXT to PDF and saved as [{pdfFileName}] in Blob Storage.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting TXT to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl; // Return the URL of the uploaded PDF
    }

    /// <inheritdoc />
    public async Task<string?> ConvertHtmlToPdfDocumentAsync(string tmpFileDownloadName)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // PDF output blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (HTML file)
                BlobClient htmlBlobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the HTML blob as a stream
                using (var htmlStream = new MemoryStream())
                {
                    await htmlBlobClient.DownloadToAsync(htmlStream).ConfigureAwait(false);
                    htmlStream.Position = 0; // Reset stream position

                    // Convert HTML to PDF
                    using (var pdfStream = new MemoryStream())
                    {
                        // Configure HTML load options for Aspose.PDF
                        var loadOptions = new Aspose.Pdf.HtmlLoadOptions();

                        // Load the HTML content into a new PDF document
                        using (var pdfDocument = new Aspose.Pdf.Document(htmlStream, loadOptions))
                        {
                            // Save the document to the output PDF stream
                            pdfDocument.Save(pdfStream);
                        }

                        // Reset stream position before uploading
                        pdfStream.Position = 0;

                        // Get a reference to the new PDF blob (output file)
                        BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                        // Upload the generated PDF to Blob Storage
                        await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                        // Get the URI of the uploaded PDF file
                        pdfFileUrl = pdfBlobClient.Uri.ToString();

                        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted HTML to PDF and saved as [{pdfFileName}] in Blob Storage.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting HTML to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl;
    }

    /// <inheritdoc />
    public async Task<string?> ConvertDocToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // PDF output blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (DOC file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the DOC blob as a stream
                using (var docStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(docStream).ConfigureAwait(false);
                    docStream.Position = 0;  // Reset stream position

                    // Create a new Document object from the DOC stream
                    var document = new Aspose.Words.Document(docStream); // No 'using' here
                    bool convertAllPages = firstPageOnly == false && document.PageCount > 1;

                    // Check if the document has at least one page
                    if (document.PageCount > 0)
                    {
                        Aspose.Words.Document wordDocument;
                        if (convertAllPages)
                        {
                            // Extract all all pages
                            wordDocument = document.ExtractPages(0, document.PageCount);
                        }
                        else
                        {
                            // Extract the first page (0-based index)
                            wordDocument = document.ExtractPages(0, 1);
                        }

                        // Memory stream to hold the generated PDF
                        using (var pdfStream = new MemoryStream())
                        {
                            // Save the first page as a PDF to the memory stream
                            wordDocument.Save(pdfStream, Aspose.Words.SaveFormat.Pdf);

                            // Reset stream position before uploading
                            pdfStream.Position = 0;

                            // Get a reference to the new PDF blob (output file)
                            BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                            // Upload the generated PDF to Blob Storage
                            await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                            // Get the URI of the uploaded PDF file
                            pdfFileUrl = pdfBlobClient.Uri.ToString();

                            string firstPageText = !convertAllPages ? "(first page)" : string.Empty;

                            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted DOC to PDF {firstPageText} and saved as [{pdfFileName}] in Blob Storage.");
                        }
                    }
                    else
                    {
                        this.logger.LogWarning($"{LoggingConstants.HskUiLogPrefix} The document does not contain any pages.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting DOC to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl;
    }

    /// <inheritdoc />
    public async Task<string?> ConvertXmlToPdfDocumentAsync(string tmpFileDownloadName)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // PDF output blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (HTML file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the XML blob as a stream
                using (var xmlStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(xmlStream).ConfigureAwait(false);
                    xmlStream.Position = 0;  // Reset stream position

                    // Load the XML content from the stream
                    using (var xmlReader = new StreamReader(xmlStream))
                    {
                        string xmlContent = await xmlReader.ReadToEndAsync().ConfigureAwait(false);

                        // Now convert the XML content to PDF using Aspose.Pdf
                        var pdfDocument = new Aspose.Pdf.Document();
                        Aspose.Pdf.Page page = pdfDocument.Pages.Add();

                        // Create a text fragment containing the XML content
                        var textFragment = new Aspose.Pdf.Text.TextFragment(xmlContent);
                        textFragment.TextState.FontSize = 12;

                        // Add the text to the page
                        page.Paragraphs.Add(textFragment);

                        // Save the PDF to a memory stream
                        using (var pdfStream = new MemoryStream())
                        {
                            pdfDocument.Save(pdfStream);
                            pdfStream.Position = 0;

                            // Get a reference to the new PDF blob (output file)
                            BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                            // Upload the PDF to Blob Storage
                            await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                            // Get the URI of the uploaded PDF file
                            pdfFileUrl = pdfBlobClient.Uri.ToString();

                            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted XML to PDF and saved as [{pdfFileName}] in Blob Storage.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting XML to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl;
    }

    /// <inheritdoc />
    public async Task<string?> ConvertXlsToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // Output PDF blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure the blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (XLS file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the XLS blob as a stream
                using (var xlsStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(xlsStream).ConfigureAwait(false);
                    xlsStream.Position = 0;  // Reset stream position

                    // Load the XLS file from the downloaded stream using Aspose.Cells
                    var workbook = new Aspose.Cells.Workbook(xlsStream);

                    var convertAllSheets = firstPageOnly && workbook.Worksheets.Count > 1;

                    // Create a PdfSaveOptions object to customize the PDF save behavior
                    var pdfSaveOptions = new Aspose.Cells.PdfSaveOptions();

                    // Set which worksheet(s) to export by making only the first sheet visible
                    for (int i = 0; i < workbook.Worksheets.Count; i++)
                    {
                        if (convertAllSheets)
                        {
                            workbook.Worksheets[i].IsVisible = true; // Mark all worksheets as visible
                        }
                        else
                        {
                            workbook.Worksheets[i].IsVisible = i == 0; // Only the first worksheet (index 0) is visible
                            break;
                        }
                    }

                    // Save the first worksheet as a PDF to a memory stream
                    using (var pdfStream = new MemoryStream())
                    {
                        workbook.Save(pdfStream, pdfSaveOptions);

                        // Reset stream position before uploading to Blob Storage
                        pdfStream.Position = 0;

                        // Get a reference to the PDF blob (output file)
                        BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                        // Upload the PDF to Blob Storage
                        await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                        // Get the URI of the uploaded PDF file
                        pdfFileUrl = pdfBlobClient.Uri.ToString();

                        string firstWorkSheetText = !convertAllSheets ? "(first sheet)" : string.Empty;

                        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted worksheets {firstWorkSheetText} of XLS to PDF and saved as [{pdfFileName}] in Blob Storage.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting XLS to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl; // Return the URL of the uploaded PDF
    }

    /// <inheritdoc />
    public async Task<string?> ConvertMsgToPdfDocumentAsync(string tmpFileDownloadName)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // Output PDF blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure the blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (MSG file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the MSG blob as a stream
                using (var msgStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(msgStream).ConfigureAwait(false);
                    msgStream.Position = 0;  // Reset stream position

                    // Load the MSG file using Aspose.Email (use Load instead of FromStream)
                    MapiMessage mapiMessage = MapiMessage.Load(msgStream);

                    // Create a MemoryStream for the PDF output
                    using (var pdfStream = new MemoryStream())
                    {
                        // Extract the email content to HTML (Aspose.Words can work with HTML)
                        string emailHtmlContent = mapiMessage.BodyHtml;

                        // Create a new Aspose.Words document from the HTML content
                        var doc = new Aspose.Words.Document(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(emailHtmlContent)));

                        // Save the document as a PDF into the memory stream
                        doc.Save(pdfStream, Aspose.Words.SaveFormat.Pdf);

                        // Reset the stream position before uploading to Blob Storage
                        pdfStream.Position = 0;

                        // Get a reference to the PDF blob (output file)
                        BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                        // Upload the PDF to Blob Storage
                        await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                        // Get the URI of the uploaded PDF file
                        pdfFileUrl = pdfBlobClient.Uri.ToString();

                        this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted MSG to PDF and saved as [{pdfFileName}] in Blob Storage.");
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting MSG to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl; // Return the URL of the uploaded PDF
    }

    /// <inheritdoc />
    public async Task<string?> ConvertPptToPdfDocumentAsync(string tmpFileDownloadName, bool firstPageOnly = true)
    {
        string pdfFileName = $"preview_{tmpFileDownloadName}.pdf"; // Output PDF blob name
        string? pdfFileUrl = null; // URL to return after upload

        // Ensure the blob service client is available
        if (this.blobServiceClient != null)
        {
            try
            {
                // Get the blob container client
                BlobContainerClient containerClient = await this.GetBlobContainerClientAsync().ConfigureAwait(true);

                // Get a reference to the blob (PPTX file)
                BlobClient blobClient = containerClient.GetBlobClient($"tmp_{tmpFileDownloadName}");

                // Download the PPTX blob as a stream
                using (var pptxStream = new MemoryStream())
                {
                    await blobClient.DownloadToAsync(pptxStream).ConfigureAwait(false);
                    pptxStream.Position = 0;  // Reset stream position
                    bool convertAllslides = false;

                    // Load the PPTX file from the downloaded stream using Aspose.Slides
                    using (var presentation = new Aspose.Slides.Presentation(pptxStream))
                    {
                        // Create a new presentation to hold the first slide
                        using (var slidePresentation = new Aspose.Slides.Presentation())
                        {
                            convertAllslides = firstPageOnly == false && presentation.Slides.Count > 1;

                            // Check if there are any slides in the original presentation
                            if (presentation.Slides.Count > 0)
                            {
                                // Clone the first slide into the new presentation
                                if (!convertAllslides)
                                {
                                    var firstSlide = presentation.Slides[0];
                                    slidePresentation.Slides.AddClone(firstSlide);
                                }

                                // Clone all slides in the presentation
                                else
                                {
                                    for (int i = 0; i < presentation.Slides.Count; i++)
                                    {
                                        slidePresentation.Slides.AddClone(presentation.Slides[i]);
                                    }
                                }
                            }

                            // Save the first slide presentation as a PDF to a new memory stream
                            using (var pdfStream = new MemoryStream())
                            {
                                slidePresentation.Save(pdfStream, Aspose.Slides.Export.SaveFormat.Pdf);

                                // Reset stream position before uploading to Blob Storage
                                pdfStream.Position = 0;

                                // Get a reference to the PDF blob (output file)
                                BlobClient pdfBlobClient = containerClient.GetBlobClient(pdfFileName);

                                // Upload the PDF to Blob Storage
                                await pdfBlobClient.UploadAsync(pdfStream, overwrite: true).ConfigureAwait(false);

                                // Get the URI of the uploaded PDF file
                                pdfFileUrl = pdfBlobClient.Uri.ToString();

                                string firstSlideText = !convertAllslides ? "(first slide)" : string.Empty;
                                this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} Converted slides {firstSlideText} of PPT to PDF and saved as [{pdfFileName}] in Blob Storage.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting PPTX to PDF and saving to Blob Storage as [{pdfFileName}].");
            }
        }

        return pdfFileUrl; // Return the URL of the uploaded PDF
    }

    /// <summary>
    /// Retrieves a <see cref="BlobContainerClient"/> for the specified blob container name and ensures that the container exists.
    /// </summary>
    /// <remarks>
    /// The blob container name is retrieved from the environment variable "BlobContainerName". If the name is null or empty,
    /// an <see cref="ArgumentNullException"/> is thrown. If the <see cref="blobServiceClient"/> is not initialized,
    /// an <see cref="InvalidOperationException"/> is thrown.
    /// </remarks>
    /// <exception cref="ArgumentNullException">Thrown when the blob container name is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the blob service client is not initialized.</exception>
    /// <returns>A <see cref="BlobContainerClient"/> for the specified container.</returns>
    private async Task<BlobContainerClient> GetBlobContainerClientAsync()
    {
        // Ensure that the blobServiceClient is not null
        if (this.blobServiceClient == null)
        {
            throw new InvalidOperationException($"{LoggingConstants.HskUiLogPrefix} BlobServiceClient is not initialized.");
        }

        // Get the blob container name from the environment or config
        string? blobContainerName = this.configuration[StorageKeys.BlobServiceContainerNameDocuments] ?? string.Empty;

        // Check if the blobContainerName is null or empty
        if (string.IsNullOrEmpty(blobContainerName))
        {
            throw new ArgumentNullException(nameof(blobContainerName), $"{LoggingConstants.HskUiLogPrefix} Blob container name cannot be null or empty.");
        }

        // Use the null-forgiving operator to tell the compiler it won't be null here
        BlobContainerClient containerClient = this.blobServiceClient.GetBlobContainerClient(blobContainerName!);

        // Ensure the container exists (create if it doesn't)
        await containerClient.CreateIfNotExistsAsync().ConfigureAwait(true);

        return containerClient;
    }

    /// <summary>
    /// Helper method to handle conversion operations with logging and exception handling.
    /// </summary>
    /// <param name="convertFunc">The conversion function to be executed.</param>
    /// <param name="conversionType">A string indicating the type of conversion.</param>
    /// <returns>The PDF file path if successful, or null if an error occurs.</returns>
    private async Task<string?> ConvertWithLoggingAsync(Func<Task<string?>> convertFunc, string conversionType)
    {
        try
        {
            return await convertFunc().ConfigureAwait(true);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Error converting {conversionType} to PDF document.");
            return null;
        }
    }

    /// <summary>
    /// Determines whether the provided content type corresponds to a raster image format.
    /// </summary>
    /// <param name="contentType">The MIME type of the file to check.</param>
    /// <returns>
    /// <c>true</c> if the content type corresponds to a raster image; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method checks against a predefined set of common raster image MIME types, including formats such as
    /// BMP, JPEG, PNG, GIF, TIFF, ICO, WEBP, and several others. The content type comparison is case-insensitive.
    /// </remarks>
    private bool IsRasterImage(string contentType)
    {
        // List of common raster image MIME types
        var rasterMimeTypes = new HashSet<string>
        {
            "image/bmp",                // BMP
            "image/jpeg",               // JPEG / JPG
            "image/png",                // PNG
            "image/gif",                // GIF
            "image/tiff",               // TIFF
            "image/x-icon",             // ICO
            "image/webp",               // WEBP
            "image/vnd.microsoft.icon", // Another ICO MIME type
            "image/x-ms-bmp",           // Another BMP MIME type
            "image/x-pcx",              // PCX
            "image/x-pict",             // PICT
            "image/x-portable-bitmap",  // PBM
            "image/x-portable-pixmap",  // PPM
            "image/x-rgb",              // RGB
            "image/x-tga",              // TGA
            "image/x-xbitmap",          // XBM
            "image/x-xpixmap",          // XPM
        };

        // Return true if the content type is in the set of raster image types
        return rasterMimeTypes.Contains(contentType.ToLower());
    }
}
