using System;
using System.Threading.Tasks;
using Common.Dto.Tracker;
using Common.Logging;
using coordinator.Domain;
using coordinator.Functions.DurableEntity.Entity;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.Orchestration.Functions.Tracker
{
    public class UpdateTrackerOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<UpdateTrackerOrchestrator> _log;
        private readonly IConfiguration _configuration;

        const string loggingName = $"{nameof(UpdateTrackerOrchestrator)} - {nameof(Run)}";

        public UpdateTrackerOrchestrator(ILogger<UpdateTrackerOrchestrator> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        [FunctionName(nameof(UpdateTrackerOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(_log);

            var payload = context.GetInput<UpdateTrackerPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var currentCaseId = payload.CaseOrchestrationPayload.CmsCaseId;

            log.LogMethodFlow(payload.CaseOrchestrationPayload.CorrelationId, loggingName, $"Retrieve tracker for case {currentCaseId}");
            var (caseTracker, caseRefreshLogsEntity) = await CreateOrGetCaseTrackersForEntity(context, currentCaseId, payload.CaseOrchestrationPayload.CorrelationId, log);

            try
            {
                caseTracker.SetValue(payload.Tracker);
            }
            catch (Exception exception)
            {
                caseTracker.RegisterCompleted((context.CurrentUtcDateTime, false));
                caseRefreshLogsEntity.LogCase((context.CurrentUtcDateTime, TrackerLogType.Failed, exception.Message));

                log.LogMethodError(payload.CaseOrchestrationPayload.CorrelationId, loggingName, $"Error when running {nameof(UpdateTrackerOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CaseOrchestrationPayload.CorrelationId, loggingName, string.Empty);
            }
        }
    }
}