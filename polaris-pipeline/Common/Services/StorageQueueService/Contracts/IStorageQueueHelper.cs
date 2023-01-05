using Azure.Storage.Queues;

namespace Common.Services.StorageQueueService.Contracts;

public interface IStorageQueueHelper
{
    QueueClient GetQueueClient(string connectionString, string queueName);

    bool IsBase64Encoded(string str);

    string Base64Encode(string plainText);
}
