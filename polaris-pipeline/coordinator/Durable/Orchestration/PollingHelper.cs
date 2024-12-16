using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System;

namespace coordinator.Durable.Orchestration;

public class PollingHelper
{
    // By default, we mean do not retry
    private static readonly RetryOptions _defaultActivityRetryOptions = new(firstRetryInterval: TimeSpan.FromSeconds(1), maxNumberOfAttempts: 1);

    public static async Task<PollingResult<T>> PollActivityUntilComplete<T>(IDurableOrchestrationContext context, PollingArgs pollingArgs)
    {
        // Pause before making the call because we want one interval before making the first call
        var firstCheckTime = context.CurrentUtcDateTime.AddMilliseconds(pollingArgs.PrePollingDelayMs);
        await context.CreateTimer(firstCheckTime, CancellationToken.None);

        var results = new List<T>();
        while (true)
        {
            // Note the two types of retry in this method:
            //  1. The activity itself is retried according to the RetryOptions passed in the PollingArgs (transient HTTP errors)
            //  2. The entire polling loop is retried until a positive flag is returned from the activity
            var (isCompleted, result) = await context.CallActivityWithRetryAsync<(bool, T)>(
                pollingArgs.ActivityName,
                pollingArgs.ActivityRetryOptions ?? _defaultActivityRetryOptions,
                pollingArgs.ActivityInput);

            results.Add(result);

            if (isCompleted)
            {
                return new PollingResult<T>
                {
                    IsCompleted = true,
                    Results = results,
                    FinalResult = result
                };
            }

            if (results.Count >= pollingArgs.MaxPollingAttempts)
            {
                return new PollingResult<T>
                {
                    IsCompleted = false,
                    Results = results,
                    FinalResult = result
                };
            }

            var nextCheck = context.CurrentUtcDateTime.AddMilliseconds(pollingArgs.PollingIntervalMs);
            await context.CreateTimer(nextCheck, CancellationToken.None);
        }
    }

    public static PollingArgs CreatePollingArgs(
        string activityName,
        object activityInput,
        int prePollingDelayMs,
        int pollingIntervalMs,
        int maxPollingAttempts,
        RetryOptions activityRetryOptions = null)
    {
        return new PollingArgs
        {
            ActivityName = activityName,
            ActivityInput = activityInput,
            PrePollingDelayMs = prePollingDelayMs,
            PollingIntervalMs = pollingIntervalMs,
            MaxPollingAttempts = maxPollingAttempts,
            ActivityRetryOptions = activityRetryOptions,
        };
    }
}

public class PollingResult<T>
{
    public bool IsCompleted { get; set; }
    public List<T> Results { get; set; }
    public T FinalResult { get; set; }
}

public class PollingArgs
{
    public string ActivityName { get; set; }
    public object ActivityInput { get; set; }
    public int PrePollingDelayMs { get; set; }
    public int PollingIntervalMs { get; set; }
    public int MaxPollingAttempts { get; set; }
    public RetryOptions ActivityRetryOptions { get; set; }
}