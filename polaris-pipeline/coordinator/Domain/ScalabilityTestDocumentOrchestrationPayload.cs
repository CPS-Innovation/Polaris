#if SCALABILITY_TEST
namespace coordinator.Domain
{
    public class ScalabilityTestDocumentOrchestrationPayload
    {
        public ScalabilityTestDocumentOrchestrationPayload(string caseId, string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }

        public string CaseId { get; init; }

        public string DocumentId { get; init; }
    }
}
#endif