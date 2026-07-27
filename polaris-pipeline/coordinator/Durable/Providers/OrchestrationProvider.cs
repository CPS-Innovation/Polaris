// <copyright file="OrchestrationProvider.cs" company="TheCrownProsecutionService">
// Copyright (c) The Crown Prosecution Service. All rights reserved.
// </copyright>

namespace coordinator.Durable.Providers;

using Azure;
using Common.Dto.Response;
using Common.Telemetry;
using coordinator.Durable.Orchestration;
using coordinator.Durable.Payloads;
using coordinator.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class OrchestrationProvider : IOrchestrationProvider
{
    private static readonly OrchestrationRuntimeStatus[] InProgressStatuses =
    [
        OrchestrationRuntimeStatus.Running,
        OrchestrationRuntimeStatus.Pending,
        OrchestrationRuntimeStatus.Suspended,
    ];

    private static readonly OrchestrationRuntimeStatus[] CompletedStatuses =
    [
        OrchestrationRuntimeStatus.Completed,
        OrchestrationRuntimeStatus.Failed,
        OrchestrationRuntimeStatus.Terminated
    ];

    private static readonly OrchestrationRuntimeStatus[] EntityStatuses =
    [
        // entities are eternally running orchestrations
        OrchestrationRuntimeStatus.Running,
    ];

    static int GetCaseIdFromInstanceId(string instanceId) => int.Parse(
        Regex.Match(instanceId, @"\d+", RegexOptions.None, TimeSpan.FromSeconds(1))
        .Value
    );

    private readonly IConfiguration configuration;
    private readonly IQueryConditionFactory queryConditionFactory;
    private readonly ILogger<OrchestrationProvider> logger;
    private readonly ITelemetryClient telemetryClient;

    public OrchestrationProvider(
            IConfiguration configuration,
            IQueryConditionFactory queryConditionFactory,
            ILogger<OrchestrationProvider> logger,
            ITelemetryClient telemetryClient)
    {
        this.configuration = configuration;
        this.queryConditionFactory = queryConditionFactory;
        this.logger = logger;
        this.telemetryClient = telemetryClient;
    }

    public static string GetKey(int caseId) => $"[{caseId}]";

    public async Task<List<int>> FindCaseInstancesByDateAsync(DurableTaskClient orchestrationClient, DateTime createdTimeTo, int batchSize)
    {
        var instanceIds = await GetInstanceIdsAsync(orchestrationClient, this.queryConditionFactory.Create(createdTimeTo, batchSize));

        return instanceIds
            .Select(GetCaseIdFromInstanceId)
            .ToList();
    }

    public async Task<bool> RefreshCaseAsync(DurableTaskClient client, Guid correlationId, int caseId, CasePayload casePayload, HttpRequest req)
    {
        var instanceId = GetKey(caseId);
        var existingInstance = await client.GetInstanceAsync(instanceId);

        if (existingInstance != null && InProgressStatuses.Contains(existingInstance.RuntimeStatus))
        {
            return false;
        }

        await client.ScheduleNewOrchestrationInstanceAsync(nameof(RefreshCaseOrchestrator), casePayload, new StartOrchestrationOptions { InstanceId = instanceId });
        return true;
    }

    public async Task<DeleteCaseOrchestrationResult> DeleteCaseOrchestrationAsync(DurableTaskClient client, int caseId)
    {
        var result = new DeleteCaseOrchestrationResult();
        try
        {
            var key = GetKey(caseId);
            var inProgressCondition = this.queryConditionFactory.Create(InProgressStatuses, key);
            var completedCondition = this.queryConditionFactory.Create(CompletedStatuses, key);
            var terminateInstanceIds = await GetInstanceIdsAsync(client, inProgressCondition);
            result.TerminatedInstancesCount = terminateInstanceIds.Count;
            result.GotTerminateInstancesDateTime = DateTime.UtcNow;

            await Task.WhenAll(terminateInstanceIds.Select(instanceId => client.TerminateInstanceAsync(instanceId, "Forcibly terminated DELETE")));
            result.TerminatedInstancesTime = DateTime.UtcNow;

            var didComplete = await WaitForOrchestrationsToCompleteAsync(client, inProgressCondition);
            result.DidOrchestrationsTerminate = didComplete;
            result.TerminatedInstancesSettledDateTime = DateTime.UtcNow;

            var orchestratorPurgeInstanceIds = await GetInstanceIdsAsync(client, completedCondition);

            result.GotPurgeInstancesDateTime = DateTime.UtcNow;
            result.PurgeInstancesCount = orchestratorPurgeInstanceIds.Count;

            foreach (var instance in orchestratorPurgeInstanceIds)
            {
                var purgeResult = await client.PurgeInstanceAsync(instance);
                result.PurgedInstancesCount += purgeResult.PurgedInstanceCount;
            }

            result.OrchestrationEndDateTime = DateTime.UtcNow;
            result.IsSuccess = true;
            return result;
        }
        catch (Exception ex)
        {
            this.telemetryClient.TrackException(ex);
            return result;
        }
    }

    public async Task<(OrchestrationProviderStatus Status, string InstanceId)> BulkSearchDocumentAsync(DurableTaskClient orchestrationClient, DocumentPayload documentPayload, CancellationToken cancellationToken = default)
    {
        var instanceId = GetKey(documentPayload);
        var orchestrationStatus = await this.GetOrchestrationProviderStatus(orchestrationClient, documentPayload, cancellationToken);

        if (orchestrationStatus != OrchestrationProviderStatus.NotStarted)
        {
            return (orchestrationStatus, instanceId);
        }

        await orchestrationClient.ScheduleNewOrchestrationInstanceAsync(
            nameof(RefreshDocumentOrchestrator),
            documentPayload,
            new StartOrchestrationOptions
            {
                InstanceId = instanceId,
            }, cancellationToken);
        return (OrchestrationProviderStatus.Initiated, instanceId);
    }

    public async Task<OrchestrationProviderStatus> GetOrchestrationProviderStatus(DurableTaskClient orchestrationClient, DocumentPayload documentPayload, CancellationToken cancellationToken = default)
    {
        try
        {
            var instanceId = GetKey(documentPayload);

            var existingInstance = await orchestrationClient.GetInstanceAsync(instanceId, cancellationToken);

            if (existingInstance != null)
            {
                if (InProgressStatuses.Contains(existingInstance.RuntimeStatus))
                {
                    return OrchestrationProviderStatus.Processing;
                }

                if (existingInstance.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
                {
                    this.logger.LogError("Bulk Redaction Search failed. Orchestration instance Id: {InstanceId}. Failed reason: {Reason}", instanceId, existingInstance.FailureDetails);

                    return OrchestrationProviderStatus.Failed;
                }

                if (CompletedStatuses.Contains(existingInstance.RuntimeStatus))
                {
                    return OrchestrationProviderStatus.Completed;
                }
            }

            return OrchestrationProviderStatus.NotStarted;
        }
        catch (Exception ex)
        {
            this.logger.LogError(
                ex,
                "Error getting orchestration status for instance {InstanceId}. Error: {ErrorMessage}",
                GetKey(documentPayload),
                ex.Message);

            return OrchestrationProviderStatus.Failed;
        }
    }

    private static string GetKey(DocumentPayload documentPayload) => $"[{documentPayload.CaseId}.{documentPayload.MaterialId}.{documentPayload.DocumentId}]";

    private static async Task<List<string>> GetInstanceIdsAsync(DurableTaskClient client, OrchestrationQuery condition)
    {
        var instanceIds = new List<string>();

        await foreach (var page in client.GetAllInstancesAsync(condition).AsPages())
        {
            instanceIds.AddRange(page.Values.Select(o => o.InstanceId));
        }

        return instanceIds;
    }

    private static async Task<bool> WaitForOrchestrationsToCompleteAsync(DurableTaskClient client, OrchestrationQuery condition)
    {
        int remainingRetryAttempts = 10;
        const int retryDelayMilliseconds = 1000;
        do
        {
            var allInstancesAreTerminated = true;
            await foreach (var page in client.GetAllInstancesAsync(condition).AsPages())
            {
                allInstancesAreTerminated &= page.Values.All(i => i.RuntimeStatus == OrchestrationRuntimeStatus.Terminated);
            }

            if (allInstancesAreTerminated)
            {
                return true;
            }

            await Task.Delay(retryDelayMilliseconds);
            remainingRetryAttempts--;

        } while (remainingRetryAttempts >= 0);

        return false;
    }
}
