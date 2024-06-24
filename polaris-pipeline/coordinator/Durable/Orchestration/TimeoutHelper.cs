using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Durable.Orchestration;

public static class TimeoutHelper
{
    public static async Task<bool> RaceAgainstTimeoutAsync(IDurableOrchestrationContext context, TimeSpan timespan, Task task)
    {
        using var cts = new CancellationTokenSource();
        var deadline = context.CurrentUtcDateTime.Add(timespan);
        var timeoutTask = context.CreateTimer(deadline, cts.Token);

        var result = await Task.WhenAny(task, timeoutTask);
        cts.Cancel();

        return result == task;
    }
}