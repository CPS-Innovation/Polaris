#if SCALABILITY_TEST
namespace coordinator.Domain
{
    public class ScalabilityTestCaseOrchestrationPayload
    {
        public ScalabilityTestCaseOrchestrationPayload(long caseId, int documentCount)
        {
            CaseId = caseId;
            DocumentCount = documentCount;
        }

        public long CaseId { get; init; }

        public int DocumentCount { get; init; }
    }
}
#endif