// <copyright file="MaterialReclassificationOrchestrationService.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace Cps.Fct.Hk.Ui.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Constants;
using Common.Dto.Request;
using Common.Dto.Request.HouseKeeping;
using Common.Dto.Response.HouseKeeping;
using Cps.Fct.Hk.Ui.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

/// <summary>
/// Provides a complete case material reclassification service.
/// </summary>
public class MaterialReclassificationOrchestrationService(
    ILogger<MaterialReclassificationOrchestrationService> logger,
    IReclassificationService reclassificationService,
    ICommunicationService communicationService,
    ICaseActionPlanService caseActionPlanService,
    IWitnessService witnessService)
    : IMaterialReclassificationOrchestrationService
{
    private readonly ILogger<MaterialReclassificationOrchestrationService> logger = logger;
    private readonly IReclassificationService reclassificationService = reclassificationService;
    private readonly ICommunicationService communicationService = communicationService;
    private readonly ICaseActionPlanService caseActionPlanService = caseActionPlanService;
    private readonly IWitnessService witnessService = witnessService;

    /// <inheritdoc/>
    public async Task<CompleteReclassificationResponse> CompleteReclassificationAsync(
        int caseId,
        int materialId,
        CmsAuthValues cmsAuthValues,
        CompleteReclassificationRequest request)
    {
        var transactionId = Guid.NewGuid();
        var renameMaterialRequest = new RenameMaterialRequest(transactionId, materialId, request.reclassification.subject);
        int newWitnessId;

        if (request?.witness?.WitnessId != null && request.HasStatement())
        {
            request.reclassification.Statement!.Witness = request.witness.WitnessId.Value;
        }

        this.logger.LogInformation(
            $"{LoggingConstants.HskUiLogPrefix} starting complete reclassification for MaterialId: [{materialId}], TransactionId: [{transactionId}]");

        var tasksToRun = new List<Task<OperationResult>>();

        Task<OperationResult>? reclassificationTask = null;
        Task<OperationResult>? actionPlanTask = null;
        Task<OperationResult>? renameMaterialTask = null;
        OperationResult? addWitnessResult = null;

        // Add witness task must be executed first when user has selected add witness flow, due to reclassification to Statement having a dependency on a new witness id.
        if (request!.AddWitness())
        {
            addWitnessResult = await this.ExecuteAddWitness(request.reclassification.urn, caseId, request.witness!, cmsAuthValues, transactionId).ConfigureAwait(false);

            if (addWitnessResult.Success)
            {
                newWitnessId = (int)addWitnessResult.ResultData!;
                request.reclassification.Statement!.Witness = newWitnessId;
            }
        }

        // Add Action plan task to collection of tasks to be run in parallel
        if (request.HasActionPlan())
        {
            actionPlanTask = this.ExecuteAddActionPlan(request.reclassification.urn, caseId, request.actionPlan!, cmsAuthValues, transactionId);
            tasksToRun.Add(actionPlanTask);
        }

        // Rename material for all other non statement reclassification.
        if (!request.HasStatement())
        {
            renameMaterialTask = this.ExecuteMaterialRename(caseId, renameMaterialRequest, cmsAuthValues, transactionId);
            tasksToRun.Add(renameMaterialTask);
        }

        // Add reclassification task to collection of tasks to be run in parallel
        reclassificationTask = this.ExecuteReclassificationAsync(caseId, materialId, request.reclassification, cmsAuthValues, transactionId);
        tasksToRun.Add(reclassificationTask);

        // Wait for all operations to complete running in parallel.
        var results = await Task.WhenAll(tasksToRun).ConfigureAwait(false);

        var operationResults = results.ToList();
        OperationResult? reclassificationResult = operationResults.FirstOrDefault(x => x.OperationName == "ReclassifyCaseMaterial");
        OperationResult? renameMaterialResult = operationResults.FirstOrDefault(x => x.OperationName == "RenameMaterial");
        OperationResult? actionPlanResult = operationResults.FirstOrDefault(x => x.OperationName == "AddCaseActionPlan");

        // Add add the result of witness operation to all operations result, if user journey flow is add witness.
        if (request.AddWitness() && addWitnessResult != null)
        {
            operationResults.Add(addWitnessResult);
        }

        // Determine overall status
        bool success = results.All(x => x.Success);
        bool failed = results.All(x => !x.Success);
        bool partialSuccess = results.Any(x => x.Success) && results.Any(x => !x.Success);
        string?[] errors = results.Select(x => x.ErrorMessage).ToArray();
        string status = success ? "Success" : (partialSuccess ? "PartialSuccess" : "Failed");

        return new CompleteReclassificationResponse(
            overallSuccess: success,
            status: status,
            materialId: materialId,
            transactionId: transactionId.ToString(),
            reclassificationResult: reclassificationResult!,
            renameMaterialResult: renameMaterialResult,
            actionPlanResult: actionPlanResult,
            addWitnessResult: addWitnessResult,
            errors: errors!);
    }

    private async Task<OperationResult> ExecuteReclassificationAsync(
        int caseId,
        int materialId,
        ReclassifyCaseMaterialRequest request,
        CmsAuthValues cmsAuthValues,
        Guid transactionId)
    {
        try
        {
            this.logger.LogInformation(
                $"{LoggingConstants.HskUiLogPrefix} executing reclassification for MaterialId: [{materialId}], TransactionId: {transactionId}");

            // Call the underlying service method directly
            ReclassificationResponse result = await this.reclassificationService.ReclassifyCaseMaterialAsync(
                caseId,
                materialId,
                request.classification,
                request.documentTypeId,
                request.used,
                request.subject,
                cmsAuthValues,
                request.Statement,
                request.Exhibit).ConfigureAwait(false);

            return new OperationResult(
                Success: true,
                OperationName: "ReclassifyCaseMaterial",
                ErrorMessage: null,
                ResultData: result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} reclassification failed for MaterialId: [{materialId}], TransactionId: [{transactionId}]");

            return new OperationResult(
                Success: false,
                OperationName: "ReclassifyCaseMaterial",
                ErrorMessage: ex.Message,
                ResultData: null);
        }
    }

    private async Task<OperationResult> ExecuteMaterialRename(
        int caseId,
        RenameMaterialRequest request,
        CmsAuthValues cmsAuthValues,
        Guid transactionId)
    {
        try
        {
            this.logger.LogInformation(
                $"{LoggingConstants.HskUiLogPrefix} executing rename for MaterialId: [{request.materialId}], TransactionId: [{transactionId}", request.materialId, request.CorrespondenceId);

            // Call the underlying service method directly
            RenameMaterialResponse result = await this.communicationService.RenameMaterialAsync(caseId, request.materialId, request.subject, cmsAuthValues).ConfigureAwait(false);

            return new OperationResult(
                Success: true,
                OperationName: "RenameMaterial",
                ErrorMessage: null,
                ResultData: result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} material rename failed for MaterialId: [{request.materialId}], TransactionId: {transactionId}", request.materialId, request.CorrespondenceId);

            return new OperationResult(
                Success: false,
                OperationName: "RenameMaterial",
                ErrorMessage: ex.Message,
                ResultData: null);
        }
    }

    private async Task<OperationResult> ExecuteAddActionPlan(
         string urn,
         int caseId,
         AddCaseActionPlanRequest request,
         CmsAuthValues cmsAuthValues,
         Guid transactionId)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} executing action plan for URN: [{urn}], TransactionId: [{transactionId}]");

            // Call the underlying service method directly
            NoContentResult result = await this.caseActionPlanService.AddCaseActionPlanAsync(urn, caseId, request, cmsAuthValues).ConfigureAwait(false);

            return new OperationResult(
                Success: true,
                OperationName: "AddCaseActionPlan",
                ErrorMessage: null,
                ResultData: result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} Action plan creation failed for URN: [{urn}], TransactionId: [{transactionId}]");

            return new OperationResult(
                Success: false,
                OperationName: "AddCaseActionPlan",
                ErrorMessage: ex.Message,
                ResultData: null);
        }
    }

    private async Task<OperationResult> ExecuteAddWitness(
        string urn,
        int caseId,
        Common.Dto.Request.HouseKeeping.WitnessRequest request,
        CmsAuthValues cmsAuthValues,
        Guid transactionId)
    {
        try
        {
            this.logger.LogInformation($"{LoggingConstants.HskUiLogPrefix} executing witness addition for CaseId: [{caseId}], TransactionId: [{transactionId}]");

            var result = await this.witnessService.AddWitnessAsync(
                urn,
                caseId,
                request.FirstName,
                request.Surname,
                cmsAuthValues,
                transactionId).ConfigureAwait(false);

            return new OperationResult(
                Success: true,
                OperationName: "AddWitness",
                ErrorMessage: null,
                ResultData: result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, $"{LoggingConstants.HskUiLogPrefix} witness addition failed for CaseId: [{caseId}], TransactionId: [{transactionId}]");

            return new OperationResult(
                Success: false,
                OperationName: "AddWitness",
                ErrorMessage: ex.Message,
                ResultData: null);
        }
    }
}
