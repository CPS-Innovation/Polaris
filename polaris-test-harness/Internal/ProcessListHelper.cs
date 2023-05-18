public static class ProcessListHelper
{
    public static async Task ProcessList<T>(Args args, IEnumerable<T> items, Func<T, int, Task> processItem)
    {
        if (args.RunInParallel)
        {
            var i = 0;
            await ConcurrentAsync(32, items, (item) => processItem(item, i++));
        }
        else
        {
            var i = 0;
            foreach (var item in items)
            {
                await processItem(item, i++);
                await Task.Delay(args.SequentialRunningMidCallDelay);
            }
        }
    }

    private static async Task ConcurrentAsync<T>(int maxConcurrency, IEnumerable<T> items, Func<T, Task> createTask)
    {
        var allTasks = new List<Task>();
        var activeTasks = new List<Task>();
        foreach (var item in items)
        {
            if (activeTasks.Count >= maxConcurrency)
            {
                var completedTask = await Task.WhenAny(activeTasks);
                activeTasks.Remove(completedTask);
            }
            var task = createTask(item);
            allTasks.Add(task);
            activeTasks.Add(task);
        }
        await Task.WhenAll(allTasks);
    }
}