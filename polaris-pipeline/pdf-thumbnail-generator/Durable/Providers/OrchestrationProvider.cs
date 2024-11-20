
using Common.Dto.Response;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using pdf_thumbnail_generator.Domain;
using pdf_thumbnail_generator.Durable.Orchestration;
using pdf_thumbnail_generator.Durable.Payloads;

namespace pdf_thumbnail_generator.Durable.Providers
{
    public class OrchestrationProvider : IOrchestrationProvider
    {
        private static readonly OrchestrationRuntimeStatus[] InProgressStatuses =
        [
            OrchestrationRuntimeStatus.Running,
            OrchestrationRuntimeStatus.Pending,
            OrchestrationRuntimeStatus.Suspended
        ];
        
        private static readonly OrchestrationRuntimeStatus[] CompletedStatuses =
        [
            OrchestrationRuntimeStatus.Completed,
            OrchestrationRuntimeStatus.Failed,
            OrchestrationRuntimeStatus.Terminated
        ];

        public async Task<List<string>> FindInstancesByDateAsync(DurableTaskClient client, DateTime createdTimeTo, int batchSize)
        {
            var query = new OrchestrationQuery
            {
                CreatedTo = createdTimeTo,
                PageSize = batchSize
            };
            
            var instanceIds = await GetInstanceIdsAsync(client, query);
            return instanceIds;
        }

        public async Task<OrchestrationStatus> GenerateThumbnailAsync(DurableTaskClient client, ThumbnailOrchestrationPayload payload)
        {
            var instanceId = ThumbnailOrchestrator.GetKey(payload.CaseId, payload.DocumentId, payload.VersionId, payload.MaxDimensionPixel);
            var existingInstance = await client.GetInstanceAsync(instanceId); 
            
            if (existingInstance != null) 
            {
                if (InProgressStatuses.Contains(existingInstance.RuntimeStatus))
                { 
                    return OrchestrationStatus.InProgress;
                }

                if (CompletedStatuses.Contains(existingInstance.RuntimeStatus))
                {
                    return OrchestrationStatus.Completed;
                }
            }

            await client.ScheduleNewOrchestrationInstanceAsync(nameof(ThumbnailOrchestrator), payload, options: new StartOrchestrationOptions
            {
                InstanceId = instanceId
            });

            return OrchestrationStatus.Accepted;
        }

        public async Task<DeleteCaseOrchestrationResult> DeleteCaseThumbnailOrchestrationAsync(DurableTaskClient client, string instanceId, DateTime earliestDateToKeep)
        {
            var result = new DeleteCaseOrchestrationResult();
            
            try 
            {
                var terminateInstanceIds = await GetInstanceIdsAsync(client, new OrchestrationQuery
                {
                    InstanceIdPrefix = instanceId,
                    Statuses = InProgressStatuses
                });

                result.TerminatedInstancesCount = terminateInstanceIds.Count;
                result.GotPurgeInstancesDateTime = DateTime.UtcNow;

                await Task.WhenAll(
                    terminateInstanceIds.Select(i => client.TerminateInstanceAsync(i, "Forcibly terminated DELETE"))
                );
                result.TerminatedInstancesTime = DateTime.UtcNow;

                var didComplete = await WaitForOrchestrationsToCompleteAsync(client, terminateInstanceIds);
                result.DidOrchestrationsTerminate = didComplete;
                result.TerminatedInstancesSettledDateTime = DateTime.UtcNow;

                var purgeResult = await client.PurgeInstancesAsync(DateTime.MinValue, earliestDateToKeep);
                result.PurgedInstancesCount = purgeResult.PurgedInstanceCount;

                result.OrchestrationEndDateTime = DateTime.UtcNow;
                result.IsSuccess = true;

                return result;
            }
            catch (Exception)
            {
                return result;
            }
        }

        private static async Task<List<string>> GetInstanceIdsAsync(DurableTaskClient client, OrchestrationQuery condition)
        { 
            var instanceIds = new List<string>();

            var instances = client.GetAllInstancesAsync(condition);
            var instanceEnumerator = instances.GetAsyncEnumerator();

            while (await instanceEnumerator.MoveNextAsync())
            {
                var instance = instanceEnumerator.Current;
                instanceIds.Add(instance.InstanceId);
            }

            return instanceIds;
        }

        private static async Task<bool> WaitForOrchestrationsToCompleteAsync(DurableTaskClient client, IReadOnlyCollection<string> instanceIds)
        { 
            var remainingRetryAttempts = 10;
            const int retryDelayMilliseconds = 1000;

            while (remainingRetryAttempts > 0)
            {
                var instances = client.GetAllInstancesAsync(
                    new OrchestrationQuery
                    {
                        InstanceIdPrefix = "@",
                        Statuses =
                        [
                            OrchestrationRuntimeStatus.Pending,
                            OrchestrationRuntimeStatus.Running,
                            OrchestrationRuntimeStatus.Terminated
                        ]
                    }
                );

                var instanceEnumerator = instances.GetAsyncEnumerator();

                // Collect statuses for relevant instance IDs
                var relevantStatuses = new Dictionary<string, OrchestrationRuntimeStatus>();
                while (await instanceEnumerator.MoveNextAsync())
                {
                    var instance = instanceEnumerator.Current;
                    if (instanceIds.Contains(instance.InstanceId))
                    {
                        relevantStatuses[instance.InstanceId] = instance.RuntimeStatus;
                    }
                }

                var allTerminated = relevantStatuses.Values.All(status => status == OrchestrationRuntimeStatus.Terminated);

                if (allTerminated)
                    return true;

                await Task.Delay(retryDelayMilliseconds);
                remainingRetryAttempts--;
            }

            return false;
        }
    }
}