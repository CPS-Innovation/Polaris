using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace coordinator.TelemetryEvents;

public class BulkRedactionSearchEvent : BaseTelemetryEvent
{
    public Guid CorrelationId;
    public long CaseId;
    public string DocumentId;
    public long VersionId;
    public string StackTrace { get; set; }

    public BulkRedactionSearchEvent(
        Guid correlationId,
        int caseId,
        string documentId,
        long versionId)
    {
        CorrelationId = correlationId;
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
    }

    public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
    {
        return (
            new Dictionary<string, string>
            {
                { nameof(CorrelationId), CorrelationId.ToString() },
                { nameof(CaseId), CaseId.ToString() },
                { nameof(DocumentId), DocumentId },
                { nameof(VersionId), VersionId.ToString() },
                { nameof(StackTrace), StackTrace },
            },
            new Dictionary<string, double?>());
    }
}