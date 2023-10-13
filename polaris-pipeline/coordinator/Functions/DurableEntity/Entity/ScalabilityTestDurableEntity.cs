#if SCALABILITY_TEST
using Common.Dto.Document;
using Common.Dto.Tracker;
using coordinator.Functions.DurableEntity.Entity.Contract;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    [JsonObject(MemberSerialization.OptIn)]
    public class ScalabilityTestDurableEntity : IScalabilityTestDurableEntity
    {
        [JsonProperty("transactionId")]
        public string TransactionId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty("status")]
        public CaseRefreshStatus Status { get; set; }

        [JsonProperty("running")]
        public DateTime? Running { get; set; }

        [JsonProperty("completed")]
        public float? Completed { get; set; }

        [JsonProperty("failed")]
        public float? Failed { get; set; }

        [JsonProperty("failedReason")]
        public string FailedReason { get; set; }

        public static string GetOrchestrationKey(long caseId)
        {
            // Avoid ambiguity of name collisions between e.g "123" and "1234", as delete operation uses a prefix, e.g. "123..."
            return $"[ScalabilityTest-{caseId}]";
        }

        public Task<DateTime> GetStartTime()
        {
            return Task.FromResult<DateTime>(Running.GetValueOrDefault());
        }

        public Task<float> GetDurationToCompleted()
        {
            return Task.FromResult<float>(Completed.GetValueOrDefault());
        }

        public void Reset(string transactionId)
        {
            TransactionId = transactionId;
            Status = CaseRefreshStatus.NotStarted;
            Running = null;
            Completed = null;
            Failed = null;
            FailedReason = null;
        }

        public Task<List<string>> GetCaseDocumentChanges(CmsDocumentDto[] documents)
        {
            var documentIds = documents.ToList().Select(x => x.DocumentId).ToList();

            return Task.FromResult(documentIds);
        }

        public void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args)
        {
            var (t, status, info) = args;

            Status = status;

            switch (status)
            {
                case CaseRefreshStatus.Running:
                    Running = t;
                    break;

                case CaseRefreshStatus.Completed:
                    if (Running != null)
                        Completed = (float)((t - Running).Value.TotalMilliseconds / 1000.0);
                    break;

                case CaseRefreshStatus.Failed:
                    if (Running != null)
                    {
                        Failed = (float)((t - Running).Value.TotalMilliseconds / 1000.0);
                        FailedReason = info;
                    }
                    break;
            }
        }

        [FunctionName(nameof(ScalabilityTestDurableEntity))]
        public static Task Run([EntityTrigger] IDurableEntityContext context)
        {
            return context.DispatchAsync<ScalabilityTestDurableEntity>();
        }
    }
}
#endif