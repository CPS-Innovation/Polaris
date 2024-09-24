using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using coordinator.Helpers;
using Common.Configuration;
using Common.Dto.Request;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Factories;
using DdeiClient.Services;
using FluentValidation;
using Common.Exceptions;

namespace coordinator.Functions
{
    public class ReclassifyDocument : BaseClient
    {
        private readonly ILogger<ReclassifyDocument> _logger;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IValidator<ReclassifyDocumentDto> _requestValidator;

        public ReclassifyDocument(
            ILogger<ReclassifyDocument> logger,
            IDdeiClient ddeiClient,
            IDdeiArgFactory ddeiArgFactory,
            IJsonConvertWrapper jsonConvertWrapper,
            IValidator<ReclassifyDocumentDto> requestValidator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        [FunctionName(nameof(ReclassifyDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.ReclassifyDocument)]
            HttpRequestMessage req,
            string caseUrn,
            int caseId,
            int documentId)
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();

                var content = await req.Content.ReadAsStringAsync();
                var reclassifyDocument = _jsonConvertWrapper.DeserializeObject<ReclassifyDocumentDto>(content);

                var validationResult = await _requestValidator.ValidateAsync(reclassifyDocument);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(reclassifyDocument));

                var cmsAuthValues = req.Headers.GetCmsAuthValues();
                var arg = _ddeiArgFactory.CreateReclassifyDocumentArgDto
                (
                    cmsAuthValues: cmsAuthValues,
                    correlationId: currentCorrelationId,
                    urn: caseUrn,
                    caseId: caseId,
                    documentId: documentId,
                    dto: reclassifyDocument
                );

                var result = await _ddeiClient.ReclassifyDocumentAsync(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(ReclassifyDocument), currentCorrelationId, ex);
            }
        }
    }
}