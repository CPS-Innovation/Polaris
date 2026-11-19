namespace PolarisGateway.TelemetryEvents
{
    /// <summary>
    /// Raised when a non-legacy endpoint receives a materialId using
    /// "CMS-" prefix, rather than a bare numeric id. Used to gauge real-world usage before the
    /// "CMS-" prefix support is removed from non-legacy endpoints (Stage 3 deprecation).
    /// </summary>
    public class CmsPrefixUsageEvent : BaseRequestEvent
    {
        public CmsPrefixUsageEvent(
            int caseId,
            string documentId,
            string operationName)
        {
            CaseId = caseId;
            DocumentId = documentId;
            OperationName = operationName;
        }
    }
}
