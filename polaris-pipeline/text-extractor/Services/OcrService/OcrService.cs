using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Common.Domain.Exceptions;
using Common.Logging;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Polly;
using Polly.Retry;
using text_extractor.Factories.Contracts;

namespace text_extractor.Services.OcrService
{
    public class OcrService : IOcrService
    {
        private const int RetryDelayInMilliseconds = 5000;
        private const int MaxRetryDelayInMilliseconds = 15000;
        private readonly ComputerVisionClient _computerVisionClient;
        private readonly ILogger<OcrService> _log;

        public OcrService(IComputerVisionClientFactory computerVisionClientFactory,
            ILogger<OcrService> log)
        {
            _computerVisionClient = computerVisionClientFactory.Create();
            _log = log;
        }

        public async Task<AnalyzeResults> GetOcrResultsAsync(Stream stream, Guid correlationId)
        {
            try
            {
                var streamPipeline = GetReadInStreamComputerVisionResiliencePipeline(correlationId);

                var streamResponse = await streamPipeline.ExecuteAsync(async token =>
                    await _computerVisionClient.ReadInStreamWithHttpMessagesAsync(stream),
                    CancellationToken.None);

                var textHeaders = streamResponse.Headers;
                var operationLocation = textHeaders.OperationLocation;
                await Task.Delay(500);

                const int numberOfCharsInOperationId = 36;
                var operationId = operationLocation[^numberOfCharsInOperationId..];

                ReadOperationResult results;

                while (true)
                {
                    var readPipeline = GetReadResultsComputerVisionResiliencePipeline(correlationId);

                    var readResponse = await readPipeline.ExecuteAsync(async token =>
                        await _computerVisionClient.GetReadResultWithHttpMessagesAsync(Guid.Parse(operationId)),
                        CancellationToken.None);

                    results = readResponse.Body;

                    if (results.Status is OperationStatusCodes.Running or OperationStatusCodes.NotStarted)
                    {
                        await Task.Delay(500);
                    }
                    else
                    {
                        break;
                    }
                }

                _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), "OCR process completed successfully");
                return results.AnalyzeResult;
            }
            catch (Exception ex)
            {
                _log.LogMethodError(correlationId, nameof(GetOcrResultsAsync), "An OCR Library exception occurred", ex);
                throw new OcrServiceException(ex.Message);
            }
            finally
            {
                _log.LogMethodExit(correlationId, nameof(GetOcrResultsAsync), string.Empty);
            }
        }

        internal ResiliencePipeline<HttpOperationHeaderResponse<ReadInStreamHeaders>> GetReadInStreamComputerVisionResiliencePipeline(Guid correlationId)
        {
            return new ResiliencePipelineBuilder<HttpOperationHeaderResponse<ReadInStreamHeaders>>()
                .AddRetry(new RetryStrategyOptions<HttpOperationHeaderResponse<ReadInStreamHeaders>>
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromMilliseconds(RetryDelayInMilliseconds),
                    MaxDelay = TimeSpan.FromMilliseconds(MaxRetryDelayInMilliseconds),
                    ShouldHandle = new PredicateBuilder<HttpOperationHeaderResponse<ReadInStreamHeaders>>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => r.Response.StatusCode == HttpStatusCode.TooManyRequests),
                    OnRetry = retryArguments =>
                    {
                        _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), $"Read in stream OCR results attempt number: {retryArguments.AttemptNumber}, {retryArguments.Outcome.Exception}");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }

        internal ResiliencePipeline<HttpOperationResponse<ReadOperationResult>> GetReadResultsComputerVisionResiliencePipeline(Guid correlationId)
        {
            return new ResiliencePipelineBuilder<HttpOperationResponse<ReadOperationResult>>()
                .AddRetry(new RetryStrategyOptions<HttpOperationResponse<ReadOperationResult>>
                {
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromMilliseconds(RetryDelayInMilliseconds),
                    ShouldHandle = new PredicateBuilder<HttpOperationResponse<ReadOperationResult>>()
                        .Handle<HttpRequestException>()
                        .HandleResult(r => r.Response.StatusCode == HttpStatusCode.TooManyRequests),
                    OnRetry = retryArguments =>
                    {
                        _log.LogMethodFlow(correlationId, nameof(GetOcrResultsAsync), $"Read OCR results attempt number: {retryArguments.AttemptNumber}, {retryArguments.Outcome.Exception}");
                        return ValueTask.CompletedTask;
                    }
                })
                .Build();
        }
    }
}