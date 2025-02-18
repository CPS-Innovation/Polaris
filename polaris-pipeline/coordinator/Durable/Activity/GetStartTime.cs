using coordinator.Services;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;

namespace coordinator.Durable.Activity;

public class GetStartTime(IStateStorageService stateStorageService)
{
    [Function(nameof(GetStartTime))]
    public async Task<DateTime> Run([ActivityTrigger] int caseId) =>
        (await stateStorageService.GetStateAsync(caseId)).Running.GetValueOrDefault();
}
