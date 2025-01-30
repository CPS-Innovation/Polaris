using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Dto.Response.Case;
using Common.Dto.Response.Case.PreCharge;
using Common.Dto.Response.Document;
using Ddei;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using coordinator.Durable.Payloads;
using Ddei.Factories;

namespace coordinator.Durable.Activity
{
    public class GetCaseDocuments
    {
        private readonly IDdeiClient _ddeiClient;
        private readonly IDdeiArgFactory _ddeiArgFactory;

        public GetCaseDocuments(
                 IDdeiClient ddeiClient,
                 IDdeiArgFactory ddeiArgFactory)
        {
            _ddeiClient = ddeiClient;
            _ddeiArgFactory = ddeiArgFactory;

        }

        [FunctionName(nameof(GetCaseDocuments))]
        public async Task<(CmsDocumentCoreDto[] CmsDocuments, PcdRequestCoreDto[] PcdRequests, DefendantsAndChargesListCoreDto DefendantAndCharges)> Run([ActivityTrigger] IDurableActivityContext context)
        {
            var payload = context.GetInput<CasePayload>();

            if (string.IsNullOrWhiteSpace(payload.Urn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.CmsAuthValues))
                throw new ArgumentException("Cms Auth Token cannot be null");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");

            var arg = _ddeiArgFactory.CreateCaseIdentifiersArg(
                payload.CmsAuthValues,
                payload.CorrelationId,
                payload.Urn,
                payload.CaseId);

            var getDocumentsTask = _ddeiClient.ListDocumentsAsync(arg);
            var getPcdRequestsTask = _ddeiClient.GetPcdRequestsAsync(arg);
            var getDefendantsAndChargesTask = _ddeiClient.GetDefendantAndChargesAsync(arg);

            await Task.WhenAll(getDocumentsTask, getPcdRequestsTask, getDefendantsAndChargesTask);

            return (getDocumentsTask.Result.ToArray(), getPcdRequestsTask.Result.ToArray(), getDefendantsAndChargesTask.Result);
        }
    }
}
