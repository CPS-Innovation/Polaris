using System;
using System.Threading.Tasks;
using AutoFixture;
using Azure.Messaging.EventGrid;
using Common.Constants;
using Common.Services.StorageQueueService.Contracts;
using Common.Wrappers;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using text_extractor.Functions;
using Xunit;

namespace text_extractor.tests.Functions;

public class HandleDocumentDeletedEventTests
{
    private readonly Fixture _fixture;
    private const string QueueName = "update-search-index-by-blob-name";
    private readonly Mock<IStorageQueueService> _mockStorageQueueService;

    private readonly HandleDocumentDeletedEvent _handleDocumentDeletedEvent;
    
    public HandleDocumentDeletedEventTests()
    {
        _fixture = new Fixture();
        _mockStorageQueueService = new Mock<IStorageQueueService>();

        var logger = new Mock<ILogger<HandleDocumentDeletedEvent>>();
        var mockJsonConverterWrapper = new Mock<IJsonConvertWrapper>();
        var mockConfiguration = new Mock<IConfiguration>();
        
        mockConfiguration.Setup(x => x[ConfigKeys.SharedKeys.UpdateSearchIndexByBlobNameQueueName]).Returns(QueueName);
        
        _handleDocumentDeletedEvent = new HandleDocumentDeletedEvent(logger.Object, mockJsonConverterWrapper.Object, mockConfiguration.Object, _mockStorageQueueService.Object);
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEvent_IsNull_ThrowArgumentNullException()
    {
        var act = async () =>
        {
           await _handleDocumentDeletedEvent.RunAsync(null, new ExecutionContext());
        };

        using (new AssertionScope())
        {
            await act.Should().ThrowAsync<ArgumentNullException>();
            _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName), 
                Times.Never);
        }
    }

    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsNotBlobDeleted_ThenTheEventIsIgnored()
    {
        var evt = _fixture.Create<EventGridEvent>();

        await _handleDocumentDeletedEvent.RunAsync(evt, new ExecutionContext());
        
        _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName), 
            Times.Never);
    }
    
    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsBlobDeleted_ButNoEventDataReceived_ThrowsNullReferenceException()
    {
        var evt = _fixture.Create<EventGridEvent>();
        evt.EventType = EventGridEvents.BlobDeletedEvent;
        evt.Data = null;
        
        var act = async () =>
        {
            await _handleDocumentDeletedEvent.RunAsync(evt, new ExecutionContext());
        };

        using (new AssertionScope())
        {
            await act.Should().ThrowAsync<NullReferenceException>();
            _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName), 
                Times.Never);
        }
    }
    
    [Fact]
    public async Task RunAsync_WhenEventGridEventType_IsBlobDeleted_AndEventDataIsReceivedAsExpected_ThenTheEventIsProcessed_UsingTheCorrectParams()
    {
        var evt = _fixture.Create<EventGridEvent>();
        evt.EventType = EventGridEvents.BlobDeletedEvent;
        
        const string eventJson = @"{
		            ""api"": ""DeleteBlob"",
                    ""clientRequestId"": ""a2b52c16-3aab-4f42-4567-321eae73f697"",
                    ""requestId"": ""810e5826-101e-0058-1834-df9e6f000000"",
                    ""eTag"": ""0x8DAAD4B298B5F20"",
                    ""contentType"": ""application/octet-stream"",
                    ""contentLength"": 56754,
                    ""blobType"": ""BlockBlob"",
                    ""url"": ""https://sacpsdevrumpolepipeline.blob.core.windows.net/documents/18848/pdfs/docCDE.pdf"",
                    ""sequencer"": ""00000000000000000000000000015EBD000000000008c2d5"",
                    ""storageDiagnostics"": {
                        ""batchId"": ""c68eb2e3-a006-003a-0034-dfe775000000""
                        }
                    }";
        evt.Data = new BinaryData(eventJson);
        
        await _handleDocumentDeletedEvent.RunAsync(evt, new ExecutionContext());
        
        _mockStorageQueueService.Verify(s => s.AddNewMessageAsync(It.IsAny<string>(), QueueName), 
            Times.Once);
    }
}