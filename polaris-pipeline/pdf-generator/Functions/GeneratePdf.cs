using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Common.Adapters;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Domain.Extensions;
using Common.Domain.Requests;
using Common.Domain.Responses;
using Common.Exceptions.Contracts;
using Common.Handlers;
using Common.Logging;
using Common.Services.BlobStorageService.Contracts;
using Common.Services.DocumentEvaluation.Contracts;
using Common.Services.DocumentExtractionService.Contracts;
using Common.Wrappers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using pdf_generator.Domain;
using pdf_generator.Services.PdfService;

namespace pdf_generator.Functions
{
    public class GeneratePdf
    {
        private readonly IAuthorizationValidator _authorizationValidator;
        private readonly IJsonConvertWrapper _jsonConvertWrapper;
        private readonly IValidatorWrapper<GeneratePdfRequest> _validatorWrapper;
        private readonly IDocumentEvaluationService _documentEvaluationService;
        private readonly IIdentityClientAdapter _identityClientAdapter;
        private readonly IDdeiDocumentExtractionService _documentExtractionService;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IPdfOrchestratorService _pdfOrchestratorService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly ILogger<GeneratePdf> _log;
        private readonly IConfiguration _configuration;

        public GeneratePdf(
             IAuthorizationValidator authorizationValidator, IJsonConvertWrapper jsonConvertWrapper, IValidatorWrapper<GeneratePdfRequest> validatorWrapper, 
             IDocumentEvaluationService documentEvaluationService, IIdentityClientAdapter identityClientAdapter, IDdeiDocumentExtractionService documentExtractionService, 
             IBlobStorageService blobStorageService, IPdfOrchestratorService pdfOrchestratorService, IExceptionHandler exceptionHandler, ILogger<GeneratePdf> logger, 
             IConfiguration configuration)
        {
            _authorizationValidator = authorizationValidator;
            _jsonConvertWrapper = jsonConvertWrapper;
            _validatorWrapper = validatorWrapper;
            _documentEvaluationService = documentEvaluationService;
            _documentExtractionService = documentExtractionService;
            _identityClientAdapter = identityClientAdapter;
            _blobStorageService = blobStorageService;
            _pdfOrchestratorService = pdfOrchestratorService;
            _exceptionHandler = exceptionHandler;
            _log = logger;
            _configuration = configuration;
        }

        [FunctionName("generate-pdf")]
        public async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "generate")] HttpRequestMessage request)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "GeneratePdf - Run";
            GeneratePdfResponse generatePdfResponse = null;

            try
            {
                request.Headers.TryGetValues(HttpHeaderKeys.CorrelationId, out var correlationIdValues);
                if (correlationIdValues == null)
                    throw new BadRequestException("Invalid correlationId. A valid GUID is required.", nameof(request));

                var correlationId = correlationIdValues.First();
                if (!Guid.TryParse(correlationId, out currentCorrelationId) || currentCorrelationId == Guid.Empty)
                        throw new BadRequestException("Invalid correlationId. A valid GUID is required.", correlationId);
                
                request.Headers.TryGetValues(HttpHeaderKeys.UpstreamTokenName, out var upstreamTokenValues);
                if (upstreamTokenValues == null)
                    throw new BadRequestException("Invalid upstream token. A valid DDEI token must be received for this request.", nameof(request));
                var upstreamToken = upstreamTokenValues.First();
                if (string.IsNullOrWhiteSpace(upstreamToken))
                    throw new BadRequestException("Invalid upstream token. A valid DDEI token must be received for this request.", nameof(request));

                _log.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                var authValidation = await _authorizationValidator.ValidateTokenAsync(request.Headers.Authorization, currentCorrelationId, 
                    PipelineScopes.GeneratePdf, PipelineRoles.EmptyRole);
                if (!authValidation.Item1)
                    throw new UnauthorizedException("Token validation failed");

                if (request.Content == null)
                    throw new BadRequestException("Request body has no content", nameof(request));

                var content = await request.Content.ReadAsStringAsync();
                if (string.IsNullOrWhiteSpace(content))
                    throw new BadRequestException("Request body cannot be null.", nameof(request));

                var pdfRequest = _jsonConvertWrapper.DeserializeObject<GeneratePdfRequest>(content);
                if (pdfRequest == null)
                    throw new BadRequestException($"An invalid message was received '{content}'", nameof(request));

                var results = _validatorWrapper.Validate(pdfRequest);
                if (results.Any())
                    throw new BadRequestException(string.Join(Environment.NewLine, results), nameof(request));
                
                var blobName = $"{pdfRequest.CaseId}/pdfs/{Path.GetFileNameWithoutExtension(pdfRequest.FileName)}.pdf";
                generatePdfResponse = new GeneratePdfResponse(blobName);

                /*_log.LogMethodFlow(currentCorrelationId, loggingName, 
                    $"Beginning document evaluation process for documentId {pdfRequest.DocumentId}, versionId {pdfRequest.VersionId}, proposedBlobName: {blobName}");
                
                var evaluateDocumentRequest = new EvaluateDocumentRequest(pdfRequest.CaseId, pdfRequest.DocumentId, pdfRequest.VersionId, blobName);
                var evaluationResult = await _documentEvaluationService.EvaluateDocumentAsync(evaluateDocumentRequest, currentCorrelationId);
                
                if (evaluationResult.EvaluationResult == DocumentEvaluationResult.DocumentUnchanged)
                {
                    generatePdfResponse.AlreadyProcessed = true;
                    return OkResponse(Serialize(generatePdfResponse));
                }*/
                
                var ddeiScope = _configuration[ConfigKeys.SharedKeys.DdeiScope];
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {ddeiScope}");
                var onBehalfOfAccessToken = await _identityClientAdapter.GetClientAccessTokenAsync(ddeiScope, currentCorrelationId);
                
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Retrieving Document from Cde for documentId: '{pdfRequest.DocumentId}'");
                
                var documentStream = await _documentExtractionService.GetDocumentAsync(pdfRequest.CaseUrn, pdfRequest.CaseId.ToString(), pdfRequest.DocumentCategory, 
                    pdfRequest.DocumentId, onBehalfOfAccessToken, upstreamToken, currentCorrelationId);

                var fileType = Path.GetExtension(pdfRequest.FileName).ToFileType();
                _log.LogMethodFlow(currentCorrelationId, loggingName, 
                    $"Processing retrieved document of type: '{fileType}'. Original file: '{pdfRequest.FileName}', with new fileName: '{blobName}'");

                var pdfStream = _pdfOrchestratorService.ReadToPdfStream(documentStream, fileType, pdfRequest.DocumentId, currentCorrelationId);
                    
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"Document converted to PDF successfully, beginning upload of '{blobName}'...");
                await _blobStorageService.UploadDocumentAsync(pdfStream, blobName, pdfRequest.CaseId.ToString(), pdfRequest.DocumentId, 
                    pdfRequest.VersionId.ToString(), currentCorrelationId);
                
                _log.LogMethodFlow(currentCorrelationId, loggingName, $"'{blobName}' uploaded successfully");
                
                return OkResponse(Serialize(generatePdfResponse));
            }
            catch (Exception exception)
            {
                return _exceptionHandler.HandleException(exception, currentCorrelationId, nameof(GeneratePdf), _log);
            }
            finally
            {
                _log.LogMethodExit(currentCorrelationId, loggingName, generatePdfResponse.ToJson());
            }
        }

        private string Serialize(object objectToSerialize)
        {
            return _jsonConvertWrapper.SerializeObject(objectToSerialize);
        }

        private static HttpResponseMessage OkResponse(string content)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content, Encoding.UTF8, MediaTypeNames.Application.Json)
            };
        }
    }
}