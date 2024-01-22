using System;
using System.Collections.Generic;
using System.Linq;
using Common.Telemetry;

namespace coordinator.TelemetryEvents
{
    public class DeletedCaseEvent : BaseTelemetryEvent
    {
        private const string IndexDeletedDurationSeconds = nameof(IndexDeletedDurationSeconds);
        private const string IndexSettledDurationSeconds = nameof(IndexSettledDurationSeconds);
        private const string GetInstancesToTerminateDurationSeconds = nameof(GetInstancesToTerminateDurationSeconds);
        private const string TerminateInstancesDurationSeconds = nameof(TerminateInstancesDurationSeconds);
        private const string BlobsDeletedDurationSeconds = nameof(BlobsDeletedDurationSeconds);
        private const string TerminatedInstancesSettledDurationSeconds = nameof(TerminatedInstancesSettledDurationSeconds);
        private const string GetInstancesToPurgeDurationSeconds = nameof(GetInstancesToPurgeDurationSeconds);
        private const string PurgedInstancesDurationSeconds = nameof(PurgedInstancesDurationSeconds);
        public Guid CorrelationId;
        public long CaseId;
        public DateTime StartTime;
        public DateTime RemovedCaseIndexTime;
        public DateTime IndexSettledTime;
        public DateTime BlobsDeletedTime;
        public DateTime GotTerminateInstancesTime;
        public DateTime TerminatedInstancesTime;
        public DateTime TerminatedInstancesSettledTime;
        public DateTime GotPurgeInstancesTime;
        public DateTime EndTime;
        public long AttemptedRemovedDocumentCount;
        public long SuccessfulRemovedDocumentCount;
        public long FailedRemovedDocumentCount;
        public bool DidWaitForIndexToSettle;
        public bool DidIndexSettle;
        public List<long> WaitRecordCounts;
        public int TerminatedInstancesCount;
        public bool DidClearBlobs;
        public bool DidOrchestrationsTerminate;
        public int PurgeInstancesCount;
        public int PurgedInstancesCount;

        public DeletedCaseEvent(
            Guid correlationId,
            long caseId,
            DateTime startTime
        )
        {
            CorrelationId = correlationId;
            CaseId = caseId;
            StartTime = startTime;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(DidWaitForIndexToSettle), DidWaitForIndexToSettle.ToString() },
                    { nameof(DidIndexSettle), DidIndexSettle.ToString() },
                    { nameof(WaitRecordCounts), string.Join(",", WaitRecordCounts ?? Enumerable.Empty<long>()) },
                    { nameof(DidClearBlobs), DidClearBlobs.ToString() },
                    { nameof(DidOrchestrationsTerminate), DidOrchestrationsTerminate.ToString() },
                },
                new Dictionary<string, double?>
                {
                    { durationSeconds, GetDurationSeconds( StartTime, EndTime) },
                    { IndexDeletedDurationSeconds, GetDurationSeconds(StartTime, RemovedCaseIndexTime) },
                    { IndexSettledDurationSeconds, GetDurationSeconds(RemovedCaseIndexTime, IndexSettledTime) },
                    { BlobsDeletedDurationSeconds, GetDurationSeconds(IndexSettledTime, BlobsDeletedTime) },
                    { GetInstancesToTerminateDurationSeconds, GetDurationSeconds(BlobsDeletedTime, GotTerminateInstancesTime) },
                    { TerminateInstancesDurationSeconds, GetDurationSeconds(GotTerminateInstancesTime, TerminatedInstancesTime) },
                    { TerminatedInstancesSettledDurationSeconds, GetDurationSeconds(TerminatedInstancesTime, TerminatedInstancesSettledTime) },
                    { GetInstancesToPurgeDurationSeconds, GetDurationSeconds(TerminatedInstancesSettledTime, GotPurgeInstancesTime) },
                    { PurgedInstancesDurationSeconds, GetDurationSeconds(GotPurgeInstancesTime, EndTime) },
                    { nameof(AttemptedRemovedDocumentCount), AttemptedRemovedDocumentCount },
                    { nameof(SuccessfulRemovedDocumentCount), SuccessfulRemovedDocumentCount },
                    { nameof(FailedRemovedDocumentCount), FailedRemovedDocumentCount },
                    { nameof(TerminatedInstancesCount), TerminatedInstancesCount },
                    { nameof(PurgeInstancesCount), PurgeInstancesCount },
                    { nameof(PurgedInstancesCount), PurgedInstancesCount },
                }
            );
        }
    }
}