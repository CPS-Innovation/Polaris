using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CaseRefreshLogsEntity : ICaseRefreshLogsEntity
    {
        [JsonProperty("case")]
        public List<TrackerLogDto> Case { get; set; } = new List<TrackerLogDto>();

        [JsonProperty("documents")]
        public Dictionary<string, List<TrackerLogDto>> Documents { get; set; } = new Dictionary<string, List<TrackerLogDto>>();

        public void LogDeltas((DateTime t, TrackerDeltasDto deltas) args)
        {
            var (t, deltas) = args;

            var logMessage = deltas.GetLogMessage();
            LogCase((t, TrackerLogType.DocumentsSynchronised, logMessage));

            foreach(TrackerCmsDocumentDto trackerCmsDocumentDto in deltas.CreatedCmsDocuments)
            {
                LogDocument((t, TrackerLogType.CmsDocumentRetrieved, trackerCmsDocumentDto.PolarisDocumentId.ToString(), "Created"));
            }
            foreach (TrackerCmsDocumentDto trackerCmsDocumentDto in deltas.UpdatedCmsDocuments)
            {
                LogDocument((t, TrackerLogType.CmsDocumentRetrieved, trackerCmsDocumentDto.PolarisDocumentId.ToString(), "Updated"));
            }
            foreach (TrackerCmsDocumentDto trackerCmsDocumentDto in deltas.DeletedCmsDocuments)
            {
                LogDocument((t, TrackerLogType.CmsDocumentRetrieved, trackerCmsDocumentDto.PolarisDocumentId.ToString(), "Deleted"));
            }

            if(deltas.CreatedDefendantsAndCharges != null)
            {
                LogDocument((t, TrackerLogType.DefendantAndChargesRetrieved, deltas.CreatedDefendantsAndCharges.PolarisDocumentId.ToString(), "Created"));
            }
            if (deltas.UpdatedDefendantsAndCharges != null)
            {
                LogDocument((t, TrackerLogType.DefendantAndChargesRetrieved, deltas.UpdatedDefendantsAndCharges.PolarisDocumentId.ToString(), "Updated"));
            }

            foreach (TrackerPcdRequestDto trackerPcdRequestDto in deltas.CreatedPcdRequests)
            {
                LogDocument((t, TrackerLogType.CmsDocumentRetrieved, trackerPcdRequestDto.PolarisDocumentId.ToString(), "Created"));
            }
            foreach (TrackerPcdRequestDto trackerPcdRequestDto in deltas.UpdatedPcdRequests)
            {
                LogDocument((t, TrackerLogType.CmsDocumentRetrieved, trackerPcdRequestDto.PolarisDocumentId.ToString(), "Updated"));
            }
            foreach (TrackerPcdRequestDto trackerPcdRequestDto in deltas.DeletedPcdRequests)
            {
                LogDocument((t, TrackerLogType.CmsDocumentRetrieved, trackerPcdRequestDto.PolarisDocumentId.ToString(), "Deleted"));
            }
        }

        public void LogCase((DateTime t, TrackerLogType status, string description) args)
        {
            var (t, status, description) = args;

            TrackerLogDto logEntry = new TrackerLogDto
            {
                Type = status.ToString(),
                TimeStamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                Description = description
            };

            Case.Insert(0, logEntry);
        }

        public void LogDocument((DateTime t, TrackerLogType status, string polarisDocumentId, string description) args)
        {
            var (t, status, polarisDocumentId, description) = args;

            TrackerLogDto logEntry = new TrackerLogDto
            {
                Type = status.ToString(),
                TimeStamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                Description = description
            };

            if (!Documents.ContainsKey(args.polarisDocumentId))
            {
                Documents.Add(polarisDocumentId, new List<TrackerLogDto>());
            }
            Documents[polarisDocumentId].Insert(0, logEntry);
        }

        [FunctionName(nameof(CaseRefreshLogsEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseRefreshLogsEntity>();
        }
    }
}
