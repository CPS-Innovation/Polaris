using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Azure.Identity;
using Azure.Storage.Queues;
using Common.Services.StorageQueueService.Contracts;

namespace Common.Services.StorageQueueService;

public class StorageQueueHelper : IStorageQueueHelper
{
    [ExcludeFromCodeCoverage]
    public QueueClient GetQueueClient(string connectionString, string queueName)
    {
        var credential = new DefaultAzureCredential();
        
        return new QueueClient(new Uri(string.Format(connectionString, queueName)), credential);
    }

    public bool IsBase64Encoded(string str)
    {
        try
        {
            // If no exception is caught, then it is possibly a base64 encoded string
            _ = Convert.FromBase64String(str);
            // perform a final check to make sure that the string was properly padded to the correct length
            return str.Replace(" ","").Length % 4 == 0;
        }
        catch
        {
            // If exception is caught, then it is not a base64 encoded string
            return false;
        }
    }

    public string Base64Encode(string plainText)
    {
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        return Convert.ToBase64String(plainTextBytes);
    }
}
