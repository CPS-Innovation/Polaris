// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName=HandleDocumentDeletedEvent

using System;
using System.Threading.Tasks;
using Azure.Messaging.EventGrid;
using Azure.Messaging.EventGrid.SystemEvents;
using Common.Constants;
using Common.Logging;
using Common.Services.CaseSearchService.Contracts;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace text_extractor.Functions;

public class HandleDocumentDeletedEvent
{
    private readonly ILogger<HandleDocumentDeletedEvent> _logger;
    private readonly ISearchIndexService _searchIndexService;

    public HandleDocumentDeletedEvent(ILogger<HandleDocumentDeletedEvent> logger, ISearchIndexService searchIndexService)
    {
        _logger = logger;
        _searchIndexService = searchIndexService;
    }

    /// <summary>
    /// Handles blob-deletion events raised by Azure Storage, when case documents grow stale either by lack of interaction with the CMS case or the CMS case is archived/closed
    /// The initial deletion is carried out via a LifeCycle Management policy setup against the storage table containing the documents 
    /// </summary>
    /// <param name="eventGridEvent"></param>
    /// <param name="context"></param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="NullReferenceException"></exception>
    [FunctionName("HandleDocumentDeletedEvent")]
    public async Task RunAsync([EventGridTrigger] EventGridEvent eventGridEvent, ExecutionContext context)
    {
        var processCompleted = false;
        const string loggerSource = "HandleDocumentDeletedEvent - EventGrid - Event";
        var correlationId = Guid.NewGuid();

        try
        {
            if (eventGridEvent == null || string.IsNullOrWhiteSpace(eventGridEvent.EventType))
            {
                throw new ArgumentNullException(nameof(eventGridEvent), "Null or invalid Event Grid Event received");
            }

            _logger.LogMethodEntry(correlationId, loggerSource, ReturnEventGridTopLevel(eventGridEvent));

            if (eventGridEvent.EventType == EventGridEvents.BlobDeletedEvent)
            {
                var eventData = eventGridEvent.Data.ToObjectFromJson<StorageBlobDeletedEventData>();
                if (eventData == null)
                    throw new NullReferenceException("Could not deserialize event data into the expected type: 'StorageBlobDeletedEventData'");

                _logger.LogMethodFlow(correlationId, loggerSource, ReturnEventGridEventLevel(eventData));

                var blobDetails = new Uri(eventData.Url).PathAndQuery.Split("/");
                var caseId = long.Parse(blobDetails[2]);
                var blobName = blobDetails[4];

                var searchIndexUpdated = $"The search index update event was received and should remove any joint references to caseId: {caseId} and blobName: '{blobName}'";
                _logger.LogMethodFlow(correlationId, loggerSource, searchIndexUpdated);

                await _searchIndexService.RemoveResultsByBlobNameAsync(caseId, blobName, correlationId);
            }
            else
            {
                var wrongMessageTypeReceived = $"Event grid event type was not of type Microsoft.Storage.BlobDeleted, but rather {eventGridEvent.EventType} - ignoring the raised event";
                _logger.LogMethodFlow(correlationId, loggerSource, wrongMessageTypeReceived);
            }

            processCompleted = true;
        }
        catch (Exception ex)
        {
            _logger.LogMethodError(correlationId, loggerSource, ex.Message, ex);
            throw;
        }
        finally
        {
            _logger.LogMethodExit(correlationId, loggerSource, $"Blob deletion event completed successfully: '{processCompleted}'");
        }
    }

    private static string ReturnEventGridTopLevel(EventGridEvent eventGridEvent)
    {
        return $@"New Event Grid Event:
            - Id=[{eventGridEvent.Id}]
            - EventType=[{eventGridEvent.EventType}]
            - EventTime=[{eventGridEvent.EventTime}]
            - Subject=[{eventGridEvent.Subject}]
            - Topic=[{eventGridEvent.Topic}]";
    }

    private static string ReturnEventGridEventLevel(StorageBlobDeletedEventData eventData)
    {
        return $@"Received {EventGridEvents.BlobDeletedEvent} Event: 
            - Api=[{eventData.Api}]
            - BlobType=[{eventData.BlobType}]
            - ClientRequestId=[{eventData.ClientRequestId}]
            - ContentType=[{eventData.ContentType}]
            - RequestId=[{eventData.RequestId}]
            - Sequencer=[{eventData.Sequencer}]
            - StorageDiagnostics=[{eventData.StorageDiagnostics.ToString()}]
            - Url=[{eventData.Url}]";
    }
}