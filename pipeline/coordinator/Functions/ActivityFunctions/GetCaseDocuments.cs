﻿using System;
using System.Threading.Tasks;
using Common.Domain.DocumentExtraction;
using Common.Domain.Extensions;
using Common.Logging;
using Common.Services.DocumentExtractionService.Contracts;
using coordinator.Domain;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;

namespace coordinator.Functions.ActivityFunctions
{
    public class GetCaseDocuments
    {
        private readonly IDdeiDocumentExtractionService _documentExtractionService;
        private readonly ILogger<GetCaseDocuments> _log;

        public GetCaseDocuments(IDdeiDocumentExtractionService documentExtractionService, ILogger<GetCaseDocuments> logger)
        {
           _documentExtractionService = documentExtractionService;
           _log = logger;
        }

        [FunctionName("GetCaseDocuments")]
        public async Task<CaseDocument[]> Run([ActivityTrigger] IDurableActivityContext context)
        {
            const string loggingName = $"{nameof(GetCaseDocuments)} - {nameof(Run)}";
            var payload = context.GetInput<GetCaseDocumentsActivityPayload>();
            
            if (payload == null)
                throw new ArgumentException("Payload cannot be null.");
            if (string.IsNullOrWhiteSpace(payload.CaseUrn))
                throw new ArgumentException("CaseUrn cannot be empty");
            if (payload.CaseId == 0)
                throw new ArgumentException("CaseId cannot be zero");
            if (string.IsNullOrWhiteSpace(payload.UpstreamToken))
                throw new ArgumentException("Upstream Token cannot be null");
            if (payload.CorrelationId == Guid.Empty)
                throw new ArgumentException("CorrelationId must be valid GUID");
            
            _log.LogMethodEntry(payload.CorrelationId, loggingName, payload.ToJson());
            var caseDetails = await _documentExtractionService.ListDocumentsAsync(payload.CaseUrn, payload.CaseId.ToString(), payload.UpstreamToken, payload.CorrelationId);
            
            _log.LogMethodExit(payload.CorrelationId, loggingName, caseDetails.ToJson());
            return caseDetails;
        }
    }
}
