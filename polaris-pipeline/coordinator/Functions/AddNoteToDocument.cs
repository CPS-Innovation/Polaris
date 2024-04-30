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
using Common.Exceptions;
using Common.Extensions;
using Common.Wrappers;
using Ddei.Factories;
using DdeiClient.Services;
using FluentValidation;

namespace coordinator.Functions
{
    public class AddNoteToDocument
    {
        private readonly ILogger<AddNoteToDocument> _logger;
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidator<AddDocumentNoteDto> _requestValidator;

        public AddNoteToDocument(ILogger<AddNoteToDocument> logger, IDdeiClient ddeiClient, IDdeiArgFactory ddeiArgFactory, IJsonConvertWrapper jsonConvertWrapper, IValidator<AddDocumentNoteDto> requestValidator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _ddeiClient = ddeiClient ?? throw new ArgumentNullException(nameof(ddeiClient));
            _ddeiArgFactory = ddeiArgFactory ?? throw new ArgumentNullException(nameof(ddeiArgFactory));
            _jsonConvertWrapper = jsonConvertWrapper ?? throw new ArgumentNullException(nameof(jsonConvertWrapper));
            _requestValidator = requestValidator ?? throw new ArgumentNullException(nameof(requestValidator));
        }

        [FunctionName(nameof(AddNoteToDocument))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddNote(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = RestApi.AddNoteToDocument)] HttpRequestMessage req,
            string caseUrn,
            int caseId,
            string documentCategory,
            int documentId
            )
        {
            Guid currentCorrelationId = default;

            try
            {
                currentCorrelationId = req.Headers.GetCorrelationId();
                var cmsAuthValues = req.Headers.GetCmsAuthValues();

                var content = await req.Content.ReadAsStringAsync();
                var note = _jsonConvertWrapper.DeserializeObject<AddDocumentNoteDto>(content);

                var validationResult = await _requestValidator.ValidateAsync(note);
                if (!validationResult.IsValid)
                    throw new BadRequestException(validationResult.FlattenErrors(), nameof(note));

                var arg = _ddeiArgFactory.CreateAddDocumentNoteArgDto(cmsAuthValues, currentCorrelationId, caseUrn, caseId, documentId, note.Text);

                var result = await _ddeiClient.AddDocumentNote(arg);

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return UnhandledExceptionHelper.HandleUnhandledException(_logger, nameof(AddNoteToDocument), currentCorrelationId, ex);
            }
        }
    }
}