using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PolarisGateway.Domain.Logging;
using PolarisGateway.Domain.Validators;
using PolarisGateway.Services;
using PolarisGateway.Domain.CaseData;
using PolarisGateway.Domain.CaseData.Args;
using PolarisGateway.Wrappers;

namespace PolarisGateway.Functions.DocumentRedaction
{
    public class DocumentRedactionCheckoutDocument : BasePolarisFunction
    {
        private readonly IDocumentService _documentService;
        private readonly ILogger<DocumentRedactionCheckoutDocument> _logger;

        public DocumentRedactionCheckoutDocument(ILogger<DocumentRedactionCheckoutDocument> logger,
                                                 IDocumentService documentService,
                                                 IAuthorizationValidator tokenValidator,
                                                 ITelemetryAugmentationWrapper telemetryAugmentationWrapper)
            : base(logger, tokenValidator, telemetryAugmentationWrapper)
        {
            _documentService = documentService ?? throw new ArgumentNullException(nameof(documentService));
            _logger = logger;
        }

        [FunctionName("DocumentRedactionCheckoutDocument")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "urns/{urn}/cases/{caseId}/documents/{documentCategory}/{documentId}/checkout")] HttpRequest req,
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
                _logger.LogMethodFlow(currentCorrelationId, loggingName, $" checkout document for caseId: {caseId}, documentId: {documentId}");

                await _documentService.CheckoutDocument(new DocumentArg
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
