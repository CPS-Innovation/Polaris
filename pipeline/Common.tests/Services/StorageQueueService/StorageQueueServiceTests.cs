using AutoFixture;
using Azure.Storage.Queues;
using Common.Services.StorageQueueService.Contracts;
using Moq;
using Xunit;

namespace Common.tests.Services.StorageQueueService;

public class StorageQueueServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<IStorageQueueHelper> _storageQueueHelperMock;
    private readonly Mock<QueueClient> _queueClientMock;

    private readonly Common.Services.StorageQueueService.StorageQueueService _sut;

    public StorageQueueServiceTests()
    {
        _fixture = new Fixture();
        _storageQueueHelperMock = new Mock<IStorageQueueHelper>();
        _queueClientMock = new Mock<QueueClient>();

        _storageQueueHelperMock.Setup(x => x.GetQueueClient(It.IsAny<string>(), It.IsAny<string>())).Returns(_queueClientMock.Object);

        _sut = new Common.Services.StorageQueueService.StorageQueueService(_fixture.Create<string>(), _storageQueueHelperMock.Object);
    }

    [Fact]
    public async Task AddNewMessage_CallsSendAsync()
    {
        var testMessage = _fixture.Create<string>();
        var queueName = _fixture.Create<string>();

        await _sut.AddNewMessageAsync(testMessage, queueName);
        
        _queueClientMock.Verify(x => x.SendMessageAsync(It.IsAny<string>()), Times.Once);
    }
}
