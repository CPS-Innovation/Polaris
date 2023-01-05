using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RumpoleGateway.Clients.OnBehalfOfTokenClient;
using RumpoleGateway.Domain.DocumentRedaction;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Helpers.Extension;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Extensions;
using RumpoleGateway.Clients.RumpolePipeline;
using RumpoleGateway.Mappers;
using RumpoleGateway.Services;
using RumpoleGateway.Domain.CaseData;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.Functions.DocumentRedaction
{
    public class DocumentRedactionSaveRedactions : BaseRumpoleFunction
    {
        private readonly IOnBehalfOfTokenClient _onBehalfOfTokenClient;
        private readonly IRedactionClient _redactionClient;
        private readonly IRedactPdfRequestMapper _redactPdfRequestMapper;
        private readonly IDocumentService _documentService;
        private readonly IBlobStorageClient _blobStorageClient;

        // private readonly IDocumentRedactionClient _documentRedactionClient;
        private readonly IConfiguration _configuration;
        private readonly IAuthorizationValidator tokenValidator;
        private readonly ILogger<DocumentRedactionSaveRedactions> _logger;

        public DocumentRedactionSaveRedactions(ILogger<DocumentRedactionSaveRedactions> logger, IOnBehalfOfTokenClient onBehalfOfTokenClient,
            //IDocumentRedactionClient documentRedactionClient,
            IRedactionClient redactionClient,
            IRedactPdfRequestMapper redactPdfRequestMapper,
            IDocumentService documentService,
            IBlobStorageClient blobStorageClient,
            IConfiguration configuration, IAuthorizationValidator tokenValidator)
            : base(logger, tokenValidator)
        {
            _onBehalfOfTokenClient = onBehalfOfTokenClient ?? throw new ArgumentNullException(nameof(onBehalfOfTokenClient));
            _redactionClient = redactionClient ?? throw new ArgumentNullException(nameof(redactionClient));
            _redactPdfRequestMapper = redactPdfRequestMapper ?? throw new ArgumentNullException(nameof(redactPdfRequestMapper));
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _blobStorageClient = blobStorageClient ?? throw new ArgumentNullException(nameof(blobStorageClient));

            // _documentRedactionClient = documentRedactionClient ?? throw new ArgumentNullException(nameof(documentRedactionClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            this.tokenValidator = tokenValidator;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [FunctionName("DocumentRedactionSaveRedactions")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "urns/{urn}/cases/{caseId}/documents/{documentCategory}/{documentId}/{*fileName}")] HttpRequest req,
            string urn,
            int caseId,
            string documentCategory,
            int documentId,
            string fileName)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "DocumentRedactionSaveRedactions - Run";
            DocumentRedactionSaveResult saveRedactionResult = null;

            try
            {
                Enum.TryParse(documentCategory, out CmsDocCategory cmsDocCategory);


                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                if (string.IsNullOrWhiteSpace(fileName))
                    return BadRequestErrorResponse("Invalid filename - not details received", currentCorrelationId, loggingName);

                var redactions = await req.GetJsonBody<DocumentRedactionSaveRequest, DocumentRedactionSaveRequestValidator>();
                if (!redactions.IsValid)
                {
                    LogInformation("Invalid redaction request", currentCorrelationId, loggingName);
                    return redactions.ToBadRequest();
                }

                var pdfPipelineScope = _configuration[ConfigurationKeys.PipelineRedactPdfScope];
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Getting an access token as part of OBO for the following scope {pdfPipelineScope}");
                var onBehalfOfAccessToken = await _onBehalfOfTokenClient.GetAccessTokenAsync(validationResult.AccessTokenValue.ToJwtString(), pdfPipelineScope, currentCorrelationId);

                //exchange access token via on behalf of for ultimate Cde access?
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Saving redaction details to the document for {caseId}, documentId {documentId}, fileName {fileName}");


                var redactPdfRequest = _redactPdfRequestMapper.Map(redactions.Value, caseId, documentId, fileName, currentCorrelationId);
                var redactionResult = await _redactionClient.RedactPdfAsync(redactPdfRequest, onBehalfOfAccessToken, currentCorrelationId);
                if (!redactionResult.Succeeded)
                {
                    _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Error Saving redaction details to the document for {caseId}, documentId {documentId}, fileName {fileName}");
                    return BadGatewayErrorResponse("Error Saving redaction details", currentCorrelationId, loggingName);
                }

                // todo: trapping when blob retrieval hasn't worked
                var pdfStream = await _blobStorageClient.GetDocumentAsync(redactionResult.RedactedDocumentName, currentCorrelationId);

                await _documentService.UploadPdf(new DocumentArg
                {
                    Urn = urn,
                    CaseId = caseId,
                    CmsDocCategory = cmsDocCategory,
                    DocumentId = documentId,
                }, pdfStream, fileName);

                return new OkResult();

            }
            catch (Exception exception)
            {
                return exception switch
                {
                    MsalException => InternalServerErrorResponse(exception, "An onBehalfOfToken exception occurred.", currentCorrelationId, loggingName),
                    HttpRequestException => InternalServerErrorResponse(exception, "A document redaction client http exception occurred.", currentCorrelationId, loggingName),
                    _ => InternalServerErrorResponse(exception, $"An unhandled exception occurred - \"{exception.Message}\"", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, saveRedactionResult.ToJson());
            }
        }
    }
}
