using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories;

public class QueryConditionFactory : IQueryConditionFactory
{
    public OrchestrationStatusQueryCondition Create(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string instanceIdPrefix) =>
    new OrchestrationStatusQueryCondition
    {
        InstanceIdPrefix = instanceIdPrefix,
        RuntimeStatus = runtimeStatuses,
    };

    public OrchestrationStatusQueryCondition Create(DateTime createdTimeTo, int batchSize) =>
    new OrchestrationStatusQueryCondition
    {
        // each case has a single entity, so if we target entities via @ prefix and use PageSize,
        //  this should be an efficient way to get unique cases that satisfy the createdTimeTo condition.

        InstanceIdPrefix = "@",
        CreatedTimeTo = createdTimeTo,
        PageSize = batchSize,
    };
}