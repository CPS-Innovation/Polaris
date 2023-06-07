using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Domain.Entity;
using System.Linq;

namespace coordinator.Functions.DurableEntity.Entity
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CaseRefreshLogsDurableEntity : ICaseRefreshLogsDurableEntity
    {
        [JsonProperty("case")]
        public List<CaseLogEntity> Case { get; set; } = new List<CaseLogEntity>();

        // PolarisDocumentId -> DocumentLogEntity
        [JsonProperty("documents")]
        public Dictionary<string, DocumentLogEntity> Documents { get; set; } = new Dictionary<string, DocumentLogEntity>();

        public void LogDeltas((DateTime t, CaseDeltasEntity deltas) args)
        {
            var (t, deltas) = args;

            var logMessage = deltas.GetLogMessage();
            LogCase((t, CaseRefreshStatus.DocumentsRetrieved, logMessage));

            foreach(CmsDocumentEntity trackerCmsDocumentDto in deltas.CreatedCmsDocuments)
            {
                LogDocument((t, DocumentLogType.Created, trackerCmsDocumentDto.PolarisDocumentId.ToString()));
            }
            foreach (CmsDocumentEntity trackerCmsDocumentDto in deltas.UpdatedCmsDocuments)
            {
                LogDocument((t, DocumentLogType.Updated, trackerCmsDocumentDto.PolarisDocumentId.ToString()));
            }
            foreach (CmsDocumentEntity trackerCmsDocumentDto in deltas.DeletedCmsDocuments)
            {
                LogDocument((t, DocumentLogType.Deleted, trackerCmsDocumentDto.PolarisDocumentId.ToString()));
            }

            if(deltas.CreatedDefendantsAndCharges != null)
            {
                LogDocument((t, DocumentLogType.Created, deltas.CreatedDefendantsAndCharges.PolarisDocumentId.ToString()));
            }
            if (deltas.UpdatedDefendantsAndCharges != null)
            {
                LogDocument((t, DocumentLogType.Updated, deltas.UpdatedDefendantsAndCharges.PolarisDocumentId.ToString()));
            }

            foreach (PcdRequestEntity trackerPcdRequestDto in deltas.CreatedPcdRequests)
            {
                LogDocument((t, DocumentLogType.Created, trackerPcdRequestDto.PolarisDocumentId.ToString()));
            }
            foreach (PcdRequestEntity trackerPcdRequestDto in deltas.UpdatedPcdRequests)
            {
                LogDocument((t, DocumentLogType.Updated, trackerPcdRequestDto.PolarisDocumentId.ToString()));
            }
            foreach (PcdRequestEntity trackerPcdRequestDto in deltas.DeletedPcdRequests)
            {
                LogDocument((t, DocumentLogType.Deleted, trackerPcdRequestDto.PolarisDocumentId.ToString()));
            }
        }

        public void LogCase((DateTime t, CaseRefreshStatus status, string description) args)
        {
            var (t, status, description) = args;

             var logEntry = new CaseLogEntity
             {
                Type = status.ToString(),
                TimeStamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz"),
                Description = description
            };

            Case.Insert(0, logEntry);
        }

        public void LogDocument((DateTime t, DocumentLogType status, string polarisDocumentId) args)
        {
            var (t, status, polarisDocumentId) = args;
            
            if (Documents.TryGetValue(args.polarisDocumentId, out var documentLogEntity))
            {
                var timestamp = DateTime.Parse(documentLogEntity.Timestamp).ToUniversalTime();
                float timespan = (float)(t - timestamp).TotalSeconds;

                switch(status)
                {
                    case DocumentLogType.PdfGenerated:
                        documentLogEntity.PdfGenerated = timespan;
                        break;

                    case DocumentLogType.PdfAlreadyGenerated:
                        documentLogEntity.PdfAlreadyGenerated = timespan;
                        break;

                    case DocumentLogType.Indexed:
                        documentLogEntity.Indexed = timespan;
                        break;
                }
            }
            else
            {
                documentLogEntity = new DocumentLogEntity();
                var timestamp = t.ToString("yyyy-MM-ddTHH\\:mm\\:ss.fffzzz");

                switch (status)
                {
                    case DocumentLogType.Created:
                        documentLogEntity.Created = timestamp;
                        break;

                    case DocumentLogType.Updated:
                        documentLogEntity.Updated = timestamp;
                        break;

                    case DocumentLogType.Deleted:
                        documentLogEntity.Deleted = timestamp;
                        break;
                }

                Documents.Add(polarisDocumentId, documentLogEntity);
            }
        }

        public Task<float?> GetMaxTimespan(DocumentLogType status)
        {
            var maxTimespan = Documents.Values.Max(x => x.GetStatusTime(status));

            return Task.FromResult(maxTimespan);
        }

        [FunctionName(nameof(CaseRefreshLogsDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<CaseRefreshLogsDurableEntity>();
        }
    }
}
