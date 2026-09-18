// <copyright file="ModifyDocument.cs" company="TheCrownProsecutionService">
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
using pdf_redactor.Services.DocumentManipulation;

/// <summary>
/// Represents a function that modifies a speicified document by removing or rotating pages based on the provided modifications.
/// This function is designed to be triggered by an HTTP POST request and is intended to be accessed via the Housekeeping UI front-end.
/// </summary>
/// <param name="exceptionHandler">The handler to manage exceptions.</param>
/// <param name="jsonConvertWrapper">The JSON serializer wrapper to handle serialization.</param>
/// <param name="documentManipulationService">The service to modify specified document as per the specification.</param>
/// <param name="logger">The logger instance used to log information and errors.</param>
/// <param name="requestValidator">The validator to verify the incoming request.</param>
public class ModifyDocument(
    IExceptionHandler exceptionHandler,
    IJsonConvertWrapper jsonConvertWrapper,
    IDocumentManipulationService documentManipulationService,
    ILogger<ModifyDocument> logger,
    IValidator<ModifyDocumentWithDocumentDto> requestValidator)
{
    private readonly IExceptionHandler exceptionHandler = exceptionHandler ?? throw new ArgumentNullException(nameof(exceptionHandler));
    private readonly IJsonConvertWrapper jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
    private readonly IDocumentManipulationService documentManipulationService = documentManipulationService ?? throw new ArgumentNullException(nameof(documentManipulationService));
    private readonly ILogger<ModifyDocument> logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IValidator<ModifyDocumentWithDocumentDto> requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));

    [Function(nameof(ModifyDocument))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ModifyDocument)] HttpRequest request, int caseId, string materialId)
    {
        var currentCorrelationId = request.Headers.GetCorrelationId();
        try
        {
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

            var modifications = this.jsonConvertWrapper.DeserializeObject<ModifyDocumentWithDocumentDto>(content);

            var validationResult = await this.requestValidator.ValidateAsync(modifications);
            if (!validationResult.IsValid)
            {
                throw new BadRequestException(validationResult.FlattenErrors(), nameof(request));
            }

            var modifiedPdfStream = await this.documentManipulationService.RemoveOrRotatePagesAsync(caseId, materialId, modifications, currentCorrelationId);

            return new FileStreamResult(modifiedPdfStream, ContentType.Pdf);
        }
        catch (Exception ex)
        {
            return this.exceptionHandler.HandleExceptionNew(ex, currentCorrelationId, nameof(ModifyDocument), this.logger);
        }
    }
}
