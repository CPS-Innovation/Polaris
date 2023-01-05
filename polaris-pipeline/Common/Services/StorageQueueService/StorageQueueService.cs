using System.Threading.Tasks;
using Common.Services.StorageQueueService.Contracts;

namespace Common.Services.StorageQueueService;

public class StorageQueueService : IStorageQueueService
{
    private readonly string _connectionString;
    private readonly IStorageQueueHelper _storageQueueHelper;

    public StorageQueueService(string connectionString, IStorageQueueHelper storageQueueHelper)
    {
        _connectionString = connectionString;
        _storageQueueHelper = storageQueueHelper;
    }
    
    public async Task AddNewMessageAsync(string messageBody, string queueName)
    {
        var queueClient = _storageQueueHelper.GetQueueClient(_connectionString, queueName);
        var validMessage = !_storageQueueHelper.IsBase64Encoded(messageBody) ? _storageQueueHelper.Base64Encode(messageBody) : messageBody;
        
        await queueClient.SendMessageAsync(validMessage);
    }
}
