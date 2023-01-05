using System.ComponentModel.DataAnnotations;
using AutoFixture;
using AutoFixture.AutoMoq;
using Azure.Storage.Queues.Models;
using Common.Constants;
using Common.Domain.Extensions;
using Common.Domain.QueueItems;
using Common.Services.SearchIndexService.Contracts;
using Common.Wrappers;
using document_evaluator.Functions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace document_evaluation.tests.Functions;

public class UpdateSearchIndexByBlobNameTests
{
    private readonly Fixture _fixture;
    private readonly Mock<ISearchIndexService> _mockSearchIndexService;
    private readonly Mock<ILogger<UpdateSearchIndexByBlobName>> _mockLogger;
    private readonly UpdateSearchIndexByBlobNameQueueItem _updateMessage;

    private QueueMessage _queueMessage;
    private readonly UpdateSearchIndexByBlobName _updateSearchIndexByBlobName;

    public UpdateSearchIndexByBlobNameTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new AutoMoqCustomization());

        _updateMessage = _fixture.Create<UpdateSearchIndexByBlobNameQueueItem>();
        _mockLogger = new Mock<ILogger<UpdateSearchIndexByBlobName>>();

        _queueMessage = QueuesModelFactory.QueueMessage(_fixture.Create<string>(), _fixture.Create<string>(), 
            _fixture.Create<UpdateSearchIndexByBlobNameQueueItem>().ToJson(), 1);
        
        var mockJsonConvertWrapper = new Mock<IJsonConvertWrapper>();
        var mockValidatorWrapper = new Mock<IValidatorWrapper<UpdateSearchIndexByBlobNameQueueItem>>();
        _mockSearchIndexService = new Mock<ISearchIndexService>();
        
        mockJsonConvertWrapper.Setup(wrapper => wrapper.DeserializeObject<UpdateSearchIndexByBlobNameQueueItem>(_queueMessage.MessageText))
            .Returns(_updateMessage);
        mockValidatorWrapper.Setup(wrapper => wrapper.Validate(It.IsAny<UpdateSearchIndexByBlobNameQueueItem>())).Returns(new List<ValidationResult>());
        
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(config => config[ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName]).Returns($"update-search-index-by-blob-name");

        _mockSearchIndexService.Setup(x => x.RemoveResultsByBlobNameAsync(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<Guid>())).Verifiable();

        _updateSearchIndexByBlobName = new UpdateSearchIndexByBlobName(mockJsonConvertWrapper.Object, mockValidatorWrapper.Object, mockConfiguration.Object, _mockSearchIndexService.Object);
    }
    
    [Theory]
    [InlineData("{}")]
    [InlineData("")]
    public async Task Run_ReturnsBadRequestWhenContentIsInvalid(string messageText)
    {
        _queueMessage = QueuesModelFactory.QueueMessage(_fixture.Create<string>(), _fixture.Create<string>(), 
            messageText, 1);
        
        await Assert.ThrowsAsync<Exception>(() => _updateSearchIndexByBlobName.RunAsync(_queueMessage, _mockLogger.Object));
    }

    [Fact]
    public async Task Run_ValidMessageReceived_ProcessesMessage()
    {
        await _updateSearchIndexByBlobName.RunAsync(_queueMessage, _mockLogger.Object);
        
        _mockSearchIndexService.Verify(x => x.RemoveResultsByBlobNameAsync(_updateMessage.CaseId, _updateMessage.BlobName, _updateMessage.CorrelationId), Times.Once);
    }
}
