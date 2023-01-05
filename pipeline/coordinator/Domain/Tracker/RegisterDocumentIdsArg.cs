using System;
using System.Collections.Generic;
using Common.Domain.DocumentEvaluation;

namespace coordinator.Domain.Tracker;

public class RegisterDocumentIdsArg
{
    public RegisterDocumentIdsArg(string caseUrn, long caseId, List<IncomingDocument> incomingDocuments, Guid correlationId)
    {
        CaseUrn = caseUrn ?? throw new ArgumentNullException(nameof(caseUrn));
        CaseId = caseId;
        IncomingDocuments = incomingDocuments ?? throw new ArgumentNullException(nameof(incomingDocuments));
        CorrelationId = correlationId;
    }

    public string CaseUrn { get; set; }

    public long CaseId { get; set; }

    public List<IncomingDocument> IncomingDocuments { get; set; }

    public Guid CorrelationId { get; set; }
}
