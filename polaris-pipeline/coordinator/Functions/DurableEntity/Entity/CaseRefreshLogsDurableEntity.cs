using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.Entity;

namespace coordinator.Functions.DurableEntity.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CaseRefreshLogsDurableEntity : ICaseRefreshLogsDurableEntity
    {
        [JsonProperty("case")]
        public List<CaseEntityLog> Case { get; set; } = new List<CaseEntityLog>();

        [JsonProperty("documents")]
        public Dictionary<string, List<DocumentLogEntity>> Documents { get; set; } = new Dictionary<string, List<DocumentLogEntity>>();

        public void LogDeltas((DateTime t, CaseDeltasEntity deltas) args)
        {
            var (t, deltas) = args;

            var logMessage = deltas.GetLogMessage();
            LogCase((t, TrackerLogType.DocumentsSynchronised, logMessage));

            foreach(CmsDocumentEntity trackerCmsDocumentDto in deltas.CreatedCmsDocuments)
            {
                LogDocument((t, TrackerLogType.CmsDocumentCreated, trackerCmsDocumentDto.PolarisDocumentId.ToString()));
            }
            foreach (CmsDocumentEntity trackerCmsDocumentDto in deltas.UpdatedCmsDocuments)
            {
                LogDocument((t, TrackerLogType.CmsDocumentUpdated, trackerCmsDocumentDto.PolarisDocumentId.ToString()));
            }
            foreach (CmsDocumentEntity trackerCmsDocumentDto in deltas.DeletedCmsDocuments)
            {
                LogDocument((t, TrackerLogType.CmsDocumentDeleted, trackerCmsDocumentDto.PolarisDocumentId.ToString()));
            }

            if(deltas.CreatedDefendantsAndCharges != null)
            {
                LogDocument((t, TrackerLogType.DefendantAndChargesCreated, deltas.CreatedDefendantsAndCharges.PolarisDocumentId.ToString()));
            }
            if (deltas.UpdatedDefendantsAndCharges != null)
            {
                LogDocument((t, TrackerLogType.DefendantAndChargesUpdated, deltas.UpdatedDefendantsAndCharges.PolarisDocumentId.ToString()));
            }

            foreach (PcdRequestEntity trackerPcdRequestDto in deltas.CreatedPcdRequests)
            {
                LogDocument((t, TrackerLogType.PcdRequestCreated, trackerPcdRequestDto.PolarisDocumentId.ToString()));
            }
            foreach (PcdRequestEntity trackerPcdRequestDto in deltas.UpdatedPcdRequests)
            {
                LogDocument((t, TrackerLogType.PcdRequestUpdated, trackerPcdRequestDto.PolarisDocumentId.ToString()));
            }
            foreach (PcdRequestEntity trackerPcdRequestDto in deltas.DeletedPcdRequests)
            {
                LogDocument((t, TrackerLogType.PcdRequestDeleted, trackerPcdRequestDto.PolarisDocumentId.ToString()));
            }
        }

        public void LogCase((DateTime t, TrackerLogType status, string description) args)
        {
            var (t, status, description) = args;

             var logEntry = new CaseEntityLog
             {
                Type = status.ToString(),
                TimeStamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                Description = description
            };

            Case.Insert(0, logEntry);
        }

        public void LogDocument((DateTime t, TrackerLogType status, string polarisDocumentId) args)
        {
            var (t, status, polarisDocumentId) = args;

            var logEntry = new DocumentLogEntity
            {
                Type = status.ToString(),
                TimeStamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz")
            };

            if (!Documents.ContainsKey(args.polarisDocumentId))
            {
                Documents.Add(polarisDocumentId, new List<DocumentLogEntity>());
            }
            Documents[polarisDocumentId].Insert(0, logEntry);
        }

        [FunctionName(nameof(CaseRefreshLogsDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseRefreshLogsDurableEntity>();
        }
    }
}
