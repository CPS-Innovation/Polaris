#if SCALABILITY_TEST
using System.Threading.Tasks;
using Common.ValueObjects;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Functions.Orchestration.Functions.Document
{
    public class ScalabilityTestDocumentOrchestrator : PolarisOrchestrator
    {
        public static string GetKey(long caseId, PolarisDocumentId polarisDocumentId)
        {
            return $"[ScalabilityTest-{caseId}]-{polarisDocumentId}";
        }

        public ScalabilityTestDocumentOrchestrator()
        {
        }

        [FunctionName(nameof(ScalabilityTestDocumentOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = await Task.FromResult(context.GetInput<ScalabilityTestDocumentOrchestrationPayload>());
        }
    }
}
#endif
