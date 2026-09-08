// <copyright file="GetTracker.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Functions;

using System.Threading.Tasks;
using Common.Configuration;
using Common.Wrappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using coordinator.Functions.DurableEntity.Entity.Mapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask.Client;
using System;
using Common.Services.BlobStorage;
using coordinator.Domain;
using Microsoft.Extensions.Configuration;
using coordinator.Durable.Providers;
using coordinator.Services;

public class GetTracker(ICaseDurableEntityMapper caseDurableEntityMapper, IStateStorageService stateStorageService)
{
    [Function(nameof(GetTracker))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> HttpStart(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = RestApi.CaseTracker)] HttpRequest req, int caseId)
    {
        try
        {
            var caseEntity = await stateStorageService.GetStateAsync(caseId);
            var documentsList = await stateStorageService.GetDurableEntityDocumentsStateAsync(caseId);

            var trackerDto = caseDurableEntityMapper.MapCase(caseEntity, documentsList);
            return new OkObjectResult(trackerDto);
        }
        catch (Exception ex)
        {
            // #23618 - Race condition: if a case orchestrator has just been kicked off then there is a possibility that 
            //  the entity calls that create (or reset) the entity are still queued up by the time the UI calls
            //  this endpoint. In this scenario, a StorageException is thrown and we are told the blob does not exist.
            //  AppInsights so far shows the orchestrator eventually executes and the entity becomes available, so
            //  lets just let the caller have the same experience as `!caseEntity.EntityExists`.
            // Note: the first implementation for the fix was to catch StorageException (which was what was in the App Insights logs).
            //  Falling back to catch Exception as we are not sure if the StorageException is the only exception that can be thrown.
            return new NotFoundObjectResult($"No Case Entity found with id '{caseId}' with exception '{ex.GetType().Name}: {ex.Message}");
        }
    }
}
