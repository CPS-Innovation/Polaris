namespace PolarisGateway.TelemetryEvents
{
    public class StatementsReorderedEvent : BaseRequestEvent
    {
        public StatementsReorderedEvent(
            long caseId)
        {
            CaseId = caseId;
        }
    }
}