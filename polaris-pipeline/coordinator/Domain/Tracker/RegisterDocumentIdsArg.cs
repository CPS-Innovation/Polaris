using System;
using System.Collections.Generic;
using System.Linq;
using Common.Domain.DocumentEvaluation;

namespace coordinator.Domain.Tracker;

public class RegisterDocumentIdsArg
{
    public RegisterDocumentIdsArg(string caseUrn, long caseId, TransitionDocument[] documents, Guid correlationId)
    {
        CaseUrn = caseUrn ?? throw new ArgumentNullException(nameof(caseUrn));
        CaseId = caseId;
        Documents = documents?.ToList() ?? throw new ArgumentNullException(nameof(documents));
        CorrelationId = correlationId;
    }

    public string CaseUrn { get; set; }

    public long CaseId { get; set; }

    public List<TransitionDocument> Documents { get; set; }

    public Guid CorrelationId { get; set; }
}
