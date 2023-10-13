using System;
using System.Linq;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;

namespace coordinator.Domain.Extensions;

public static class OrchestrationExtensions
{
    public static bool IsClearDownCandidate(this string runtimeStatus, params OrchestrationRuntimeStatus[] qualifyingStatuses)
    {
        if (qualifyingStatuses == null || qualifyingStatuses.Length == 0)
            return false;

        return qualifyingStatuses.Any(qualifyingStatus => string.Equals(runtimeStatus,
                Enum.GetName(qualifyingStatus), StringComparison.InvariantCultureIgnoreCase));
    }
}