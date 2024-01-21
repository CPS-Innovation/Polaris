using System;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Factories;

public interface IQueryConditionFactory
{
    public OrchestrationStatusQueryCondition Create(IEnumerable<OrchestrationRuntimeStatus> runtimeStatuses, string instanceIdPrefix);

    public OrchestrationStatusQueryCondition Create(DateTime createdTimeTo, int batchSize);
}