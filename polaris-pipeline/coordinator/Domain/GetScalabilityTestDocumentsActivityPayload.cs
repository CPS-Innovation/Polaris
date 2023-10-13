#if SCALABILITY_TEST
namespace coordinator.Domain
{
    public class GetScalabilityTestDocumentsActivityPayload
    {
        public GetScalabilityTestDocumentsActivityPayload(int documentCount)
        {
            DocumentCount = documentCount;
        }

        public int DocumentCount { get; set; }
    }
}
#endif
