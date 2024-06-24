using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Logging;
using Common.Telemetry;
using coordinator.Constants;
using coordinator.Domain.Exceptions;
using coordinator.Durable.Activity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.TelemetryEvents;
using coordinator.Validators;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace coordinator.Durable.Orchestration
{
    public class RefreshCaseOrchestrator : BaseOrchestrator
    {
        private readonly ILogger<RefreshCaseOrchestrator> _log;
        private readonly IConfiguration _configuration;
        private readonly ICmsDocumentsResponseValidator _cmsDocumentsResponseValidator;
        private readonly ITelemetryClient _telemetryClient;
        private readonly TimeSpan _timeout;
        public RefreshCaseOrchestrator(
            ILogger<RefreshCaseOrchestrator> log,
            IConfiguration configuration,
            ICmsDocumentsResponseValidator cmsDocumentsResponseValidator,
            ITelemetryClient telemetryClient)
        {
            _log = log;
            _configuration = configuration;
            _cmsDocumentsResponseValidator = cmsDocumentsResponseValidator;
            _telemetryClient = telemetryClient;
            _timeout = TimeSpan.FromSeconds(double.Parse(_configuration[ConfigKeys.CoordinatorOrchestratorTimeoutSecs]));
        }

        [FunctionName(nameof(RefreshCaseOrchestrator))]
        public async Task Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>()
                ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);
            var caseEntity = CreateEntityProxy(context, payload.CmsCaseId);
            RefreshedCaseEvent telemetryEvent = default;

            try
            {
                // Allow ourselves to timeout the entire case orchestration process by wrapping the steps and racing the wrapped 
                //  task against against a timeout
                async Task WrapCaseOrchestratorTask()
                {
                    var versionId = await caseEntity.InitialiseRefresh();

                    telemetryEvent = new RefreshedCaseEvent(
                        correlationId: payload.CorrelationId,
                        caseId: payload.CmsCaseId,
                        versionId: versionId,
                        startTime: context.CurrentUtcDateTime
                    );

                    var documents = await GetDocuments(context, payload);
                    caseEntity.SetCaseDocumentsRetrieved(context.CurrentUtcDateTime);
                    telemetryEvent.CmsDocsCount = documents.CmsDocuments.Length;
                    if (!_cmsDocumentsResponseValidator.Validate(documents.CmsDocuments))
                    {
                        throw new CaseOrchestrationException("Invalid cms documents response: duplicate document ids detected.");
                    }

                    var deltas = await caseEntity.MutateAndReturnDeltas(documents);
                    telemetryEvent.CmsDocsProcessedCount = deltas.CmsDocsProcessedCount;
                    telemetryEvent.PcdRequestsProcessedCount = deltas.PcdRequestsProcessedCount;

                    var documentPayloads = CreateDocumentPayloads(payload, deltas);
                    var documentTasks = CreateDocumentTasks(context, documentPayloads);
                    await Task.WhenAll(documentTasks);
                }

                var didOrchestrationComplete = await TimeoutHelper.RaceAgainstTimeoutAsync(context, _timeout, WrapCaseOrchestratorTask());
                if (didOrchestrationComplete)
                {
                    var endTime = context.CurrentUtcDateTime;
                    caseEntity.SetCaseCompleted(endTime);
                    telemetryEvent.EndTime = endTime;

                    _telemetryClient.TrackEvent(telemetryEvent);
                    return;
                }
                else
                {
                    throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
                }
            }
            catch (Exception exception)
            {
                caseEntity.SetCaseFailed(context.CurrentUtcDateTime);

                log.LogMethodError(payload.CorrelationId, nameof(RefreshCaseOrchestrator), $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                // todo: this gets called multiple times in a replaying scenario, should be refactored to only call once
                _telemetryClient.TrackEventFailure(telemetryEvent);
            }
        }

        private static List<CaseDocumentOrchestrationPayload> CreateDocumentPayloads(CaseOrchestrationPayload caseDocumentPayload, CaseDeltasEntity deltas)
        {
            var payloads = new List<CaseDocumentOrchestrationPayload>();
            payloads.AddRange(
                Enumerable.Concat(deltas.CreatedCmsDocuments, deltas.UpdatedCmsDocuments)
                    .Select
                    (
                        trackerCmsDocument =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                cmsAuthValues: caseDocumentPayload.CmsAuthValues,
                                correlationId: caseDocumentPayload.CorrelationId,
                                cmsCaseUrn: caseDocumentPayload.CmsCaseUrn,
                                cmsCaseId: caseDocumentPayload.CmsCaseId,
                                serializedTrackerCmsDocumentDto: JsonSerializer.Serialize(trackerCmsDocument.Item1),
                                serializedTrackerPcdRequestDto: null,
                                serializedTrackerDefendantAndChargesDto: null,
                                documentDeltaType: trackerCmsDocument.Item2
                            );
                        }
                    )
            );

            payloads.AddRange(
                Enumerable.Concat(deltas.CreatedPcdRequests, deltas.UpdatedPcdRequests)
                    .Select
                    (
                        trackerPcdRequest =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                cmsAuthValues: caseDocumentPayload.CmsAuthValues,
                                correlationId: caseDocumentPayload.CorrelationId,
                                cmsCaseUrn: caseDocumentPayload.CmsCaseUrn,
                                cmsCaseId: caseDocumentPayload.CmsCaseId,
                                serializedTrackerCmsDocumentDto: null,
                                serializedTrackerPcdRequestDto: JsonSerializer.Serialize(trackerPcdRequest),
                                serializedTrackerDefendantAndChargesDto: null,
                                documentDeltaType: DocumentDeltaType.RequiresIndexing
                            );
                        }
                    )
            );

            var dacDelta = deltas.CreatedDefendantsAndCharges ?? deltas.UpdatedDefendantsAndCharges;
            if (dacDelta != null)
            {
                payloads.Add(
                    new CaseDocumentOrchestrationPayload
                    (
                        cmsAuthValues: caseDocumentPayload.CmsAuthValues,
                        correlationId: caseDocumentPayload.CorrelationId,
                        cmsCaseUrn: caseDocumentPayload.CmsCaseUrn,
                        cmsCaseId: caseDocumentPayload.CmsCaseId,
                        serializedTrackerCmsDocumentDto: null,
                        serializedTrackerPcdRequestDto: null,
                        serializedTrackerDefendantAndChargesDto: JsonSerializer.Serialize(dacDelta),
                        documentDeltaType: DocumentDeltaType.RequiresIndexing
                    )
                );
            }

            return payloads;
        }

        private List<Task<RefreshDocumentResult>> CreateDocumentTasks
            (
                IDurableOrchestrationContext context,
                IEnumerable<CaseDocumentOrchestrationPayload> payloads
            )
        {
            return payloads
                .Select(
                    payload => context.CallSubOrchestratorAsync<RefreshDocumentResult>
                    (
                        nameof(RefreshDocumentOrchestrator),
                        InstanceIdHelper.DocumentOrchestratorKey(payload.CmsCaseId.ToString(), payload.PolarisDocumentId.ToString()),
                        payload
                    )
                ).ToList();
        }

        private async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>
            GetDocuments(IDurableOrchestrationContext context, CaseOrchestrationPayload payload)
        {
            var getCaseEntitiesActivityPayload = new GetCaseDocumentsActivityPayload(
                payload.CmsCaseUrn,
                payload.CmsCaseId,
                payload.CmsAuthValues,
                payload.CorrelationId);

            return await context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>
                (nameof(GetCaseDocuments), getCaseEntitiesActivityPayload);
        }
    }
}