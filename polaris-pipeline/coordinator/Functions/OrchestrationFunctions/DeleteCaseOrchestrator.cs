using System;
using System.Threading.Tasks;
using Common.Logging;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.OrchestrationFunctions
{
    public class DeleteCaseOrchestrator : PolarisOrchestrator
    {
        private readonly ILogger<DeleteCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;

        const string loggingName = $"{nameof(DeleteCaseOrchestrator)} - {nameof(Run)}";

        public DeleteCaseOrchestrator(ILogger<DeleteCaseOrchestrator> log, IConfiguration configuration)
        {
            _log = log;
            _configuration = configuration;
        }

        [FunctionName(nameof(DeleteCaseOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var log = context.CreateReplaySafeLogger(_log);

            var payload = context.GetInput<CaseOrchestrationPayload>();
            if (payload == null)
                throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var currentCaseId = payload.CmsCaseId;

            log.LogMethodFlow(payload.CorrelationId, loggingName, $"Retrieve tracker for case {currentCaseId}");
            var tracker = CreateOrGetTracker(context, currentCaseId, payload.CorrelationId, log);

            try
            {
                await tracker.RegisterDeleted();
            }
            catch (Exception exception)
            {
                await tracker.RegisterFailed();
                log.LogMethodError(payload.CorrelationId, loggingName, $"Error when running {nameof(DeleteCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                throw;
            }
            finally
            {
                log.LogMethodExit(payload.CorrelationId, loggingName, string.Empty);
            }
        }
    }
}