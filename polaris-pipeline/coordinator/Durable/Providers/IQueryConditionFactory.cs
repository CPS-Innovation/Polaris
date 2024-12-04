using Microsoft.DurableTask.Client;
using System;
using System.Collections.Generic;

namespace coordinator.Durable.Providers;

public interface IQueryConditionFactory
{
    public OrchestrationQuery Create(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string instanceIdPrefix);

    public OrchestrationQuery Create(DateTime createdTimeTo, int batchSize);
}