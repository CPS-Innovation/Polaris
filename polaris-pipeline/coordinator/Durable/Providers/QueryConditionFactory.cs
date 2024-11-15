using System;
using System.Collections.Generic;
using Microsoft.DurableTask.Client;

namespace coordinator.Durable.Providers;

public class QueryConditionFactory : IQueryConditionFactory
{
    public OrchestrationQuery Create(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string instanceIdPrefix) =>
    new OrchestrationQuery
    {
        InstanceIdPrefix = instanceIdPrefix,
        Statuses = runtimeStatuses,
    };

    public OrchestrationQuery Create(DateTime createdTimeTo, int batchSize) =>
    new OrchestrationQuery
    {
        // each case has a single entity, so if we target entities via @ prefix and use PageSize,
        //  this should be an efficient way to get unique cases that satisfy the createdTimeTo condition.

        InstanceIdPrefix = "@",
        CreatedTo = createdTimeTo,
        PageSize = batchSize,
    };
}