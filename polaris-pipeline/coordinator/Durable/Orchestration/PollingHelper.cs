using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.DurableTask;

namespace coordinator.Durable.Orchestration;

public class PollingHelper
{
    // By default, we mean do not retry
    public static readonly TaskOptions DefaultActivityOptions = new(new TaskRetryOptions(new RetryPolicy(firstRetryInterval: TimeSpan.FromSeconds(1), maxNumberOfAttempts: 1)));

    public static async Task<PollingResult<T>> PollActivityUntilComplete<T>(TaskOrchestrationContext context, PollingArgs pollingArgs)
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
            var (isCompleted, result) = await context.CallActivityAsync<(bool, T)>(
                pollingArgs.ActivityName,
                pollingArgs.ActivityInput,
                pollingArgs.ActivityOptions ?? DefaultActivityOptions);

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
        TaskOptions activityOptions = null)
    {
        return new PollingArgs
        {
            ActivityName = activityName,
            ActivityInput = activityInput,
            PrePollingDelayMs = prePollingDelayMs,
            PollingIntervalMs = pollingIntervalMs,
            MaxPollingAttempts = maxPollingAttempts,
            ActivityOptions = activityOptions,
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
    public TaskOptions ActivityOptions { get; set; }
}