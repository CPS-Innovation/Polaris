using System;

namespace Common.Dto.Request;

public class BulkRedactionSearchDto
{
    public string Urn { get; set; }
    public int CaseId { get; set; }
    public string DocumentId { get; set; }
    public long VersionId { get; set; }
    public string SearchText { get; set; }
    public string CmsAuthValues { get; set; }
    public Guid CorrelationId { get; set; }
}