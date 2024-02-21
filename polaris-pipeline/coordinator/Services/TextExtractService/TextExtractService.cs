using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Dto.Response;
using Common.Logging;
using coordinator.Clients.Contracts;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace coordinator.Services.TextExtractService
{
    public class TextExtractService : ITextExtractService
    {
        private const int RetryDelayInMilliseconds = 500;
        private const int MaxRetryInMilliseconds = 10000;
        private readonly ILogger<TextExtractService> _log;
        private readonly ITextExtractorClient _textExtractorClient;
        private long _targetCount;
        private readonly List<long> _recordCounts;

        public TextExtractService(ILogger<TextExtractService> log, ITextExtractorClient textExtractorClient)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _textExtractorClient = textExtractorClient ?? throw new ArgumentNullException(nameof(textExtractorClient));
            _recordCounts = new List<long>();
        }

        public async Task<IndexSettledResult> WaitForDocumentStoreResultsAsync(string caseUrn, long cmsCaseId, string cmsDocumentId, long versionId, long targetCount, Guid correlationId)
        {
            _targetCount = targetCount;

            var indexCountPipeline = GetIndexCountResiliencePipeline(correlationId);

            await indexCountPipeline.ExecuteAsync(async token =>
                await _textExtractorClient.GetDocumentIndexCount(caseUrn, cmsCaseId, cmsDocumentId, versionId, correlationId),
                CancellationToken.None);

            return new IndexSettledResult
            {
                TargetCount = _targetCount,
                IsSuccess = _recordCounts.Any() && _recordCounts.Last() == _targetCount,
                RecordCounts = _recordCounts
            };
        }

        public async Task<IndexSettledResult> WaitForCaseEmptyResultsAsync(string caseUrn, long cmsCaseId, Guid correlationId)
        {
            _targetCount = 0;

            var indexCountPipeline = GetIndexCountResiliencePipeline(correlationId);

            await indexCountPipeline.ExecuteAsync(async token =>
                await _textExtractorClient.GetCaseIndexCount(caseUrn, cmsCaseId, correlationId),
                CancellationToken.None);

            return new IndexSettledResult
            {
                TargetCount = _targetCount,
                IsSuccess = _recordCounts.Any() && _recordCounts.Last() == _targetCount,
                RecordCounts = _recordCounts
            };
        }

        internal ResiliencePipeline<SearchIndexCountResult> GetIndexCountResiliencePipeline(Guid correlationId)
        {
            return new ResiliencePipelineBuilder<SearchIndexCountResult>()
                .AddRetry(IncorrectCountRetries(correlationId))
                .Build();
        }

        private bool IsExpectedIndexCount(SearchIndexCountResult searchIndexCount)
        {
            _recordCounts.Add(searchIndexCount.LineCount);
            return _targetCount == searchIndexCount.LineCount;
        }

        private RetryStrategyOptions<SearchIndexCountResult> IncorrectCountRetries(Guid correlationId)
        {
            return new RetryStrategyOptions<SearchIndexCountResult>
            {
                MaxRetryAttempts = 5,
                MaxDelay = TimeSpan.FromMilliseconds(MaxRetryInMilliseconds),
                UseJitter = true,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(RetryDelayInMilliseconds),
                ShouldHandle = new PredicateBuilder<SearchIndexCountResult>()
                    .HandleResult(searchIndexCount => !IsExpectedIndexCount(searchIndexCount)),
                OnRetry = retryArguments =>
                {
                    _log.LogMethodFlow(correlationId, nameof(WaitForDocumentStoreResultsAsync), $"Get document index count attempt number: {retryArguments.AttemptNumber}, {retryArguments.Outcome.Exception}");
                    return ValueTask.CompletedTask;
                }
            };
        }
    }
}