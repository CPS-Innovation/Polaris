// <copyright file="RedactPdf.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace pdf_redactor.Functions;

using Common.Configuration;
using Common.Constants;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Handlers;
using Common.Wrappers;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using pdf_redactor.Services.DocumentRedaction;

/// <summary>
/// Represents a function that performs redaction on a case material document based on the provided redaction specifications.
/// This function is designed to be triggered by an HTTP POST request and is intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <param name="exceptionHandler">The handler to manage exceptions.</param>
/// <param name="jsonConvertWrapper">Wrapper to manage json serilization.</param>
/// <param name="documentRedactionService">The service that manages redaction of the requested document.</param>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="requestValidator">The validator to verify the incoming request.</param>
public class RedactPdf(
    IExceptionHandler exceptionHandler,
    IJsonConvertWrapper jsonConvertWrapper,
    IDocumentRedactionService documentRedactionService,
    ILogger<RedactPdf> logger,
    IValidator<RedactPdfRequestWithDocumentDto> requestValidator)
{
    private readonly IExceptionHandler exceptionHandler = exceptionHandler;
    private readonly IJsonConvertWrapper jsonConvertWrapper = jsonConvertWrapper;
    private readonly IDocumentRedactionService documentRedactionService = documentRedactionService;
    private readonly ILogger<RedactPdf> logger = logger;
    private readonly IValidator<RedactPdfRequestWithDocumentDto> requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));

    [Function(nameof(RedactPdf))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RedactDocument)] HttpRequest request,
        int caseId,
        string materialId,
        long documentId)
    {
        Guid currentCorrelationId = default;

        try
        {
            currentCorrelationId = request.Headers.GetCorrelationId();

            request.EnableBuffering();

            if (request.ContentLength == null || !request.Body.CanSeek)
            {
                throw new BadRequestException("Request body has no content", nameof(request));
            }

            request.Body.Seek(0, SeekOrigin.Begin);
            string content;
            using (var stream = new StreamReader(request.Body))
            {
                content = await stream.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new BadRequestException("Request body cannot be null or an empty JSON message", nameof(request));
            }

            var redactions = this.jsonConvertWrapper.DeserializeObject<RedactPdfRequestWithDocumentDto>(content);

            var validationResult = await this.requestValidator.ValidateAsync(redactions);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));
            }

            var redactPdfStream = await this.documentRedactionService.RedactAsync(caseId, materialId, redactions, currentCorrelationId);

            return new FileStreamResult(redactPdfStream, ContentType.Pdf);
        }
        catch (Exception ex)
        {
            return this.exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(RedactPdf), this.logger);
        }
    }
}
