using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using RumpoleGateway.Domain.Logging;
using RumpoleGateway.Domain.Validators;
using RumpoleGateway.Services;
using RumpoleGateway.Domain.CaseData;
using RumpoleGateway.Domain.CaseData.Args;

namespace RumpoleGateway.Functions.DocumentRedaction
{
    public class DocumentRedactionCancelCheckoutDocument : BaseRumpoleFunction
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentRedactionCancelCheckoutDocument> _logger;

        public DocumentRedactionCancelCheckoutDocument(ILogger<DocumentRedactionCancelCheckoutDocument> logger, IDocumentService documentService, IAuthorizationValidator tokenValidator)
            : base(logger, tokenValidator)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _logger = logger;
        }

        [FunctionName("DocumentRedactionCancelCheckoutDocument")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "urns/{urn}/cases/{caseId}/documents/{documentCategory}/{documentId}/checkout")] HttpRequest req,
            string urn,
            int caseId,
            string documentCategory,
            int documentId)
        {
            Guid currentCorrelationId = default;
            const string loggingName = "DocumentRedactionCheckInDocument - Run";
            try
            {
                Enum.TryParse(documentCategory, out CmsDocCategory cmsDocCategory);

                var validationResult = await ValidateRequest(req, loggingName, ValidRoles.UserImpersonation);
                if (validationResult.InvalidResponseResult != null)
                    return validationResult.InvalidResponseResult;

                currentCorrelationId = validationResult.CurrentCorrelationId;
                _logger.LogMethodEntry(currentCorrelationId, loggingName, string.Empty);

                //exchange access token via on behalf of?
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $"Cancel checkout document for caseId: {caseId}, documentId: {documentId}");

                await _documentService.CancelCheckoutDocument(new DocumentArg
                {
                    Urn = urn,
                    CaseId = caseId,
                    CmsDocCategory = cmsDocCategory,
                    DocumentId = documentId,
                });

                return new OkResult();
            }
            catch (Exception exception)
            {
                return exception switch
                {
                    _ => InternalServerErrorResponse(exception, "An unhandled exception occurred.", currentCorrelationId, loggingName)
                };
            }
            finally
            {
                _logger.LogMethodExit(currentCorrelationId, loggingName, null);
            }
        }
    }
}
