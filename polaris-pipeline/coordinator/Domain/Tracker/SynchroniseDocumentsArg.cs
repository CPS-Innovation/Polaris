using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using System;
using System.Collections.Generic;
using System.Linq;

namespace coordinator.Domain.Tracker;

public class SynchroniseDocumentsArg
{
    public SynchroniseDocumentsArg(DateTime currentUtcDateTime, string caseUrn, long caseId, DocumentDto[] documents, PcdRequestDto[] pcdRequests, Guid correlationId)
    {
        CurrentUtcDateTime = currentUtcDateTime;
        CaseUrn = caseUrn ?? throw new ArgumentNullException(nameof(caseUrn));
        CaseId = caseId;
        Documents = documents?.ToList() ?? throw new ArgumentNullException(nameof(documents));
        PcdRequests = pcdRequests?.ToList() ?? throw new ArgumentNullException(nameof(pcdRequests));
        CorrelationId = correlationId;
    }

    public DateTime CurrentUtcDateTime { get; set; }

    public string CaseUrn { get; set; }

    public long CaseId { get; set; }

    public List<DocumentDto> Documents { get; set; }

    public List<PcdRequestDto> PcdRequests { get; set; }

    public Guid CorrelationId { get; set; }
}
