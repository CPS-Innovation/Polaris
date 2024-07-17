using System;
using System.Net.Http;
using System.Threading.Tasks;
using Common.Configuration;
using Common.Dto.Request;
using Common.Exceptions;
using Common.Extensions;
using Common.Wrappers;
using coordinator.Helpers;
using Ddei.Factories;
using DdeiClient.Services;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions
{
    public class RenameDocument
    {
        private readonly ILogger<RenameDocument> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<RenameDocumentDto> _requestValidator;

        public RenameDocument(ILogger<RenameDocument> logger, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory, IJsonConvertWrapper jsonConvertWrapper, IValidator<RenameDocumentDto> requestValidator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        [FunctionName(nameof(RenameDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = RestApi.RenameDocument)] HttpRequestMessage req,
            string caseUrn,
            int caseId,
            int documentId
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var content = await req.Content.ReadAsStringAsync();
                var renameDocument = _jsonConvertWrapper.DeserializeObject<RenameDocumentDto>(content);

                var validationResult = await _requestValidator.ValidateAsync(renameDocument);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(renameDocument));

                var arg = _ddeiArgFactory.CreateRenameDocumentArgDto(cmsAuthValues, currentCorrelationId, caseUrn, caseId, documentId, renameDocument.DocumentName);

                var result = await _ddeiClient.RenameDocumentAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(RenameDocument), currentCorrelationId, ex);
            }
        }
    }
}