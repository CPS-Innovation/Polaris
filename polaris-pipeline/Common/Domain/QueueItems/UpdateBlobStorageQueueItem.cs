using System;
using System.ComponentModel.DataAnnotations;

namespace Common.Domain.QueueItems;

public class UpdateBlobStorageQueueItem
{
    public UpdateBlobStorageQueueItem(string blobName, Guid correlationId)
    {
        BlobName = blobName;
        CorrelationId = correlationId;
    }
    
    [Required] public string BlobName { get; set; }
    
    [Required] public Guid CorrelationId { get; set; }
}
