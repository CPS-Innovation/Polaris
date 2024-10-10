﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Common.Domain.Document;
using Common.Dto.Case;
using Common.Dto.Case.PreCharge;
using Common.Dto.Document;
using Common.Dto.Response;
using Common.Dto.Tracker;
using Common.Logging;
using Common.Telemetry;
using coordinator.Constants;
using coordinator.Domain.Exceptions;
using coordinator.Durable.Activity;
using coordinator.Durable.Entity;
using coordinator.Durable.Payloads;
using coordinator.Durable.Payloads.Domain;
using coordinator.TelemetryEvents;
using coordinator.Validators;
using Mapster;
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
        public static string GetKey(string caseId) => $"[{caseId}]";

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
        public async Task<TrackerDto> Run([OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var payload = context.GetInput<CaseOrchestrationPayload>()
                ?? throw new ArgumentException("Orchestration payload cannot be null.", nameof(context));

            var log = context.CreateReplaySafeLogger(_log);

            var caseEntity = await CreateOrGetCaseDurableEntity(context, payload.CmsCaseId, true, payload.CorrelationId, log);
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            RefreshedCaseEvent telemetryEvent = default;
            try
            {
                telemetryEvent = new RefreshedCaseEvent(
                    correlationId: payload.CorrelationId,
                    caseId: payload.CmsCaseId,
                    versionId: await caseEntity.GetVersion(),
                    startTime: await caseEntity.GetStartTime()
                );

                var orchestratorTask = RunCaseOrchestrator(context, caseEntity, payload, telemetryEvent);

                using var cts = new CancellationTokenSource();
                var deadline = context.CurrentUtcDateTime.Add(_timeout);
                var timeoutTask = context.CreateTimer(deadline, cts.Token);

                var result = await Task.WhenAny(orchestratorTask, timeoutTask);
                if (result == orchestratorTask)
                {
                    // success case
                    cts.Cancel();
                    _telemetryClient.TrackEvent(telemetryEvent);
                    return await orchestratorTask;
                }

                throw new TimeoutException($"Orchestration with id '{context.InstanceId}' timed out.");
            }
            catch (Exception exception)
            {
                caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Failed, exception.Message));

                log.LogMethodError(payload.CorrelationId, nameof(RefreshCaseOrchestrator), $"Error when running {nameof(RefreshCaseOrchestrator)} orchestration with id '{context.InstanceId}'", exception);
                _telemetryClient.TrackEventFailure(telemetryEvent);
                throw;
            }
        }

        private async Task<TrackerDto> RunCaseOrchestrator(IDurableOrchestrationContext context, ICaseDurableEntity caseEntity, CaseOrchestrationPayload payload, RefreshedCaseEvent telemetryEvent)
        {
            caseEntity.Reset(context.InstanceId);
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Running, null));

            var documents = await GetDocuments(context, payload);
            telemetryEvent.CmsDocsCount = documents.CmsDocuments.Length;
            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.DocumentsRetrieved, null));

            var log = context.CreateReplaySafeLogger(_log);
            var (documentTasks, cmsDocsProcessedCount, pcdRequestsProcessedCount) = await GetDocumentTasks(context, caseEntity, payload, documents, log);
            telemetryEvent.CmsDocsProcessedCount = cmsDocsProcessedCount;
            telemetryEvent.PcdRequestsProcessedCount = pcdRequestsProcessedCount;
            await Task.WhenAll(documentTasks.Select(BufferCall));

            if (await caseEntity.AllDocumentsFailed())
                throw new CaseOrchestrationException("CMS Documents, PCD Requests or Defendants and Charges failed to process during orchestration.");

            caseEntity.SetCaseStatus((context.CurrentUtcDateTime, CaseRefreshStatus.Completed, null));

            telemetryEvent.EndTime = context.CurrentUtcDateTime;

            return caseEntity.Adapt<TrackerDto>();
        }

        private async Task<(List<Task<RefreshDocumentResult>>, int, int)> GetDocumentTasks
            (
                IDurableOrchestrationContext context,
                ICaseDurableEntity caseTracker,
                CaseOrchestrationPayload caseDocumentPayload,
                (CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges) documents,
                ILogger log
            )
        {
            var now = context.CurrentUtcDateTime;

            var deltas = await caseTracker.GetCaseDocumentChanges((documents.CmsDocuments, documents.PcdRequests, documents.DefendantsAndCharges));
            var deltaLogMessage = deltas.GetLogMessage();
            log.LogMethodFlow(caseDocumentPayload.CorrelationId, nameof(RefreshCaseOrchestrator), deltaLogMessage);

            var createdOrUpdatedDocuments = deltas.CreatedCmsDocuments.Concat(deltas.UpdatedCmsDocuments).ToList();
            var createdOrUpdatedPcdRequests = deltas.CreatedPcdRequests.Concat(deltas.UpdatedPcdRequests).ToList();
            var createdOrUpdatedDefendantsAndCharges = deltas.CreatedDefendantsAndCharges ?? deltas.UpdatedDefendantsAndCharges;

            var cmsDocumentPayloads
                = createdOrUpdatedDocuments
                    .Select
                    (
                        trackerCmsDocument =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                cmsAuthValues: caseDocumentPayload.CmsAuthValues,
                                correlationId: caseDocumentPayload.CorrelationId,
                                subCorrelationId: context.NewGuid(),
                                cmsCaseUrn: caseDocumentPayload.CmsCaseUrn,
                                cmsCaseId: caseDocumentPayload.CmsCaseId,
                                serializedTrackerCmsDocumentDto: JsonSerializer.Serialize(trackerCmsDocument.Item1),
                                serializedTrackerPcdRequestDto: null,
                                serializedTrackerDefendantAndChargesDto: null,
                                documentDeltaType: trackerCmsDocument.Item2
                            );
                        }
                    )
                    .ToList();

            var pcdRequestsPayloads
                = createdOrUpdatedPcdRequests
                    .Select
                    (
                        trackerPcdRequest =>
                        {
                            return new CaseDocumentOrchestrationPayload
                            (
                                cmsAuthValues: caseDocumentPayload.CmsAuthValues,
                                correlationId: caseDocumentPayload.CorrelationId,
                                subCorrelationId: context.NewGuid(),
                                cmsCaseUrn: caseDocumentPayload.CmsCaseUrn,
                                cmsCaseId: caseDocumentPayload.CmsCaseId,
                                serializedTrackerCmsDocumentDto: null,
                                serializedTrackerPcdRequestDto: JsonSerializer.Serialize(trackerPcdRequest),
                                serializedTrackerDefendantAndChargesDto: null,
                                documentDeltaType: DocumentDeltaType.RequiresIndexing
                            );
                        }
                    ).
                    ToList();

            var defendantsAndChargesPayloads = new List<CaseDocumentOrchestrationPayload>();
            if (createdOrUpdatedDefendantsAndCharges != null)
            {
                var polarisDocumentId = PolarisDocumentIdHelper.GetPolarisDocumentId(PolarisDocumentType.DefendantsAndCharges, caseDocumentPayload.CmsCaseId.ToString());
                var payload = new CaseDocumentOrchestrationPayload
                (
                    cmsAuthValues: caseDocumentPayload.CmsAuthValues,
                    correlationId: caseDocumentPayload.CorrelationId,
                    subCorrelationId: context.NewGuid(),
                    cmsCaseUrn: caseDocumentPayload.CmsCaseUrn,
                    cmsCaseId: caseDocumentPayload.CmsCaseId,
                    serializedTrackerCmsDocumentDto: null,
                    serializedTrackerPcdRequestDto: null,
                    serializedTrackerDefendantAndChargesDto: JsonSerializer.Serialize(new DefendantsAndChargesEntity(polarisDocumentId, new DefendantsAndChargesListDto { })),
                    documentDeltaType: DocumentDeltaType.RequiresIndexing
                );
                defendantsAndChargesPayloads.Add(payload);
            }

            var allPayloads = cmsDocumentPayloads.Concat(pcdRequestsPayloads).Concat(defendantsAndChargesPayloads);

            var allTasks = allPayloads.Select
                    (
                        payload => context.CallSubOrchestratorAsync<RefreshDocumentResult>
                        (
                            nameof(RefreshDocumentOrchestrator),
                            RefreshDocumentOrchestrator.GetKey(payload.CmsCaseId, payload.PolarisDocumentId),
                            payload
                        )
                    );

            return (allTasks.ToList(), createdOrUpdatedDocuments.Count, createdOrUpdatedPcdRequests.Count);
        }

        private static async Task<T> BufferCall<T>(Task<T> task)
        {
            try
            {
                return await task;
            }
            catch (Exception)
            {
                return default;
            }
        }

        private async Task<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>
            GetDocuments(IDurableOrchestrationContext context, CaseOrchestrationPayload payload)
        {
            var getCaseEntitiesActivityPayload = new GetCaseDocumentsActivityPayload(payload.CmsCaseUrn, payload.CmsCaseId, payload.CmsAuthValues, payload.CorrelationId);
            var documents = await context.CallActivityAsync<(CmsDocumentDto[] CmsDocuments, PcdRequestDto[] PcdRequests, DefendantsAndChargesListDto DefendantsAndCharges)>(nameof(GetCaseDocuments), getCaseEntitiesActivityPayload);
            if (!_cmsDocumentsResponseValidator.Validate(documents.CmsDocuments))
            {
                throw new CaseOrchestrationException("Invalid cms documents response: duplicate document ids detected.");
            }
            return documents;
        }
    }
}