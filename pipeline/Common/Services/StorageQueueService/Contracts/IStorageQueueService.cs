using System.Threading.Tasks;

namespace Common.Services.StorageQueueService.Contracts
{
    public interface IStorageQueueService
    {
        Task AddNewMessageAsync(string messageBody, string queueName);
    }
}
