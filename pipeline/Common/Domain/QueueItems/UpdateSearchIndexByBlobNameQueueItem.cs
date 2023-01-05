using System;
using System.ComponentModel.DataAnnotations;
using Common.Validators;

namespace Common.Domain.QueueItems
{
    public class UpdateSearchIndexByBlobNameQueueItem
    {
        public UpdateSearchIndexByBlobNameQueueItem(long caseId, string blobName, Guid correlationId)
        {
            CaseId = caseId;
            BlobName = blobName;
            CorrelationId = correlationId;
        }
    
        [RequiredLongGreaterThanZero]
        public long CaseId { get; set; }

        [Required]
        public string BlobName { get; set; }
    
        [Required]
        public Guid CorrelationId { get; set; }
    }
}
