using Microsoft.Win32;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace coordinator.Durable.Orchestration;

public class PollingHelper
{
    // By default, we mean do not retry
    private static readonly RetryOptions _defaultRetryOptions = new(firstRetryInterval: TimeSpan.FromSeconds(1), maxNumberOfAttempts: 1);
    public static async Task<(List<T>, T)> PollActivityUntilComplete<T>(IDurableOrchestrationContext context, PollingArgs pollingArgs, RetryOptions retryOptions = null)
    {
        var results = new List<T>();
        while (true)
        {
            var nextCheck = context.CurrentUtcDateTime.AddMilliseconds(pollingArgs.PollingIntervalMs);
            await context.CreateTimer(nextCheck, CancellationToken.None);

            var (isCompleted, result) = await context.CallActivityWithRetryAsync<(bool, T)>(
                pollingArgs.ActivityName,
                retryOptions ?? _defaultRetryOptions,
                pollingArgs.ActivityInput);

            results.Add(result);

            if (isCompleted)
            {
                return (results, result);
            }
        }
    }

    public static PollingArgs CreatePollingArgs(string activityName, int pollingIntervalMs, object activityInput)
    {
        return new PollingArgs
        {
            ActivityName = activityName,
            ActivityInput = activityInput,
            PollingIntervalMs = pollingIntervalMs
        };
    }
}

public class PollingArgs
{
    public string ActivityName { get; set; }
    public object ActivityInput { get; set; }
    public int PollingIntervalMs { get; set; }
}