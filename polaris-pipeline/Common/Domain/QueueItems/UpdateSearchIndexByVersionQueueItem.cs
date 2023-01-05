using System;
using System.ComponentModel.DataAnnotations;
using Common.Validators;

namespace Common.Domain.QueueItems;

public class UpdateSearchIndexByVersionQueueItem
{
    public UpdateSearchIndexByVersionQueueItem(long caseId, string documentId, long versionId, Guid correlationId)
    {
        CaseId = caseId;
        DocumentId = documentId;
        VersionId = versionId;
        CorrelationId = correlationId;
    }
    
    [RequiredLongGreaterThanZero]
    public long CaseId { get; set; }

    [Required]
    public string DocumentId { get; set; }
    
    [RequiredLongGreaterThanZero]
    public long VersionId { get; set; }
    
    [Required]
    public Guid CorrelationId { get; set; }
}
