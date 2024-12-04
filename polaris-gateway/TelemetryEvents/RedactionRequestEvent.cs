using System.Collections.Generic;

namespace PolarisGateway.TelemetryEvents
{
    public class RedactionRequestEvent : BaseRequestEvent
    {
        public RedactionRequestEvent(
            int caseId,
            string documentId)
        {
            CaseId = caseId;
            DocumentId = documentId;
        }

        public bool IncludesDeletedPages => DeletedPageCount > 0;
        public int DeletedPageCount { get; set; }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            var baseProps = base.ToTelemetryEventProps();

            baseProps.Item1.Add(nameof(IncludesDeletedPages), IncludesDeletedPages.ToString());
            baseProps.Item2.Add(nameof(DeletedPageCount), DeletedPageCount);

            return baseProps;
        }
    }
}