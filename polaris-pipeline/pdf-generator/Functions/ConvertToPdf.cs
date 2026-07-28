// <copyright file="ConvertToPdf.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_generator.Functions;

using System;
using System.Net;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Logging;
using Common.Streaming;
using Common.Telemetry;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using pdf_generator.Extensions;
using pdf_generator.Services.PdfService;
using pdf_generator.TelemetryEvents;

/// <summary>
/// Represents a function that converts a case material in PNG format to PDF format.
/// This function is designed to be triggered by an HTTP POST request and is intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ConvertToPdf"/> class.
/// </remarks>
/// <param name="pdfOrchestratorService">The service used to orchestrate PDF conversion by selecting appropriate format-specific PDF service for the input document.</param>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="telemetryClient">The telemetry client used to track application events and metrics.</param>
public class ConvertToPdf
{
    private const string LoggingName = nameof(ConvertToPdf);

    private readonly IPdfOrchestratorService _pdfOrchestratorService;
    private readonly ILogger<ConvertToPdf> _logger;
    private readonly ITelemetryClient _telemetryClient;

    public ConvertToPdf(
         IPdfOrchestratorService pdfOrchestratorService,
         ILogger<ConvertToPdf> logger,
         ITelemetryClient telemetryClient)
    {
        this._pdfOrchestratorService = pdfOrchestratorService;
        this._logger = logger;
        this._telemetryClient = telemetryClient;
    }

    [ProducesResponseType((int)HttpStatusCode.OK)]
    [ProducesResponseType((int)HttpStatusCode.UnsupportedMediaType)]
    [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
    [Function(nameof(ConvertToPdf))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ConvertToPdf)] HttpRequest request,
        int caseId,
        string materialId,
        string documentId)
    {
        Guid currentCorrelationId = default;
        currentCorrelationId = request.Headers.GetCorrelationId();

        var telemetryEvent = new ConvertedDocumentEvent(currentCorrelationId)
        {
            OperationName = nameof(ConvertToPdf),
        };
        try
        {
            var fileType = ConvertToPdfHelper.GetFileType(request.Headers);

            telemetryEvent.FileType = fileType.ToString();
            telemetryEvent.CaseId = caseId.ToString();
            telemetryEvent.DocumentId = materialId;
            telemetryEvent.VersionId = documentId;

            var startTime = DateTime.UtcNow;
            telemetryEvent.StartTime = startTime;

            if (request.Body == null)
            {
                throw new BadRequestException("An empty document stream was received from the Coordinator", nameof(request));
            }

            var inputStream = await request.Body
                .EnsureSeekableAsync(); // Aspose demands a seekable stream, and as we want to record the size of the stream, we need to ensure it is seekable also.

            var originalBytes = inputStream.Length;
            telemetryEvent.OriginalBytes = originalBytes;

            var conversionResult = await this._pdfOrchestratorService.ReadToPdfStreamAsync(inputStream, fileType, materialId, currentCorrelationId);

            // #25834 - Successfully converted documents may still have a failure reason we need to record
            if (conversionResult.HasFailureReason())
            {
                telemetryEvent.FailureReason = conversionResult.GetFailureReason();
            }

            if (conversionResult.ConversionStatus == PdfConversionStatus.DocumentConverted)
            {
                var bytes = conversionResult.ConvertedDocument.Length;

                telemetryEvent.Bytes = bytes;
                telemetryEvent.EndTime = DateTime.UtcNow;
                telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();

                this._telemetryClient.TrackEvent(telemetryEvent);

                return new FileStreamResult(conversionResult.ConvertedDocument, "application/pdf")
                {
                    FileDownloadName = $"{nameof(ConvertToPdf)}.pdf",
                };
            }

            telemetryEvent.ConversionHandler = conversionResult.ConversionHandler.GetEnumValue();
            this._telemetryClient.TrackEventFailure(telemetryEvent);

            return new ObjectResult(conversionResult.ConversionStatus)
            {
                StatusCode = (int)HttpStatusCode.UnsupportedMediaType,
            };
        }
        catch (Exception exception)
        {
            this._logger.LogMethodError(currentCorrelationId, LoggingName, exception.Message, exception);

            telemetryEvent.FailureReason = exception.Message;
            this._telemetryClient.TrackEventFailure(telemetryEvent);

            return new ObjectResult(exception.ToFormattedString())
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
            };
        }
    }
}
