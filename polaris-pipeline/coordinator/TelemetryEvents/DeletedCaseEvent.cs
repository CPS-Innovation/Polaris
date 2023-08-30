using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace coordinator.TelemetryEvents
{
    public class DeletedCaseEvent : BaseTelemetryEvent
    {
        private const string indexDeletedDurationSeconds = nameof(indexDeletedDurationSeconds);
        private const string indexSettledDurationSeconds = nameof(indexSettledDurationSeconds);
        private const string getInstancesToTerminateDurationSeconds = nameof(getInstancesToTerminateDurationSeconds);
        private const string terminateInstancesDurationSeconds = nameof(terminateInstancesDurationSeconds);
        private const string purgedInstancesDurationSeconds = nameof(purgedInstancesDurationSeconds);

        public Guid CorrelationId;
        public long CaseId;

        public DateTime StartTime;
        public DateTime RemovedCaseIndexTime;
        public DateTime IndexSettledTime;
        public DateTime GotTerminateInstancesTime;
        public DateTime TerminatedInstancesTime;
        public DateTime EndTime;
        public int TerminatedInstancesCount;

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

        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(CaseId), CaseId.ToString() },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                },
                new Dictionary<string, double>
                {
                    { durationSeconds, GetDurationSeconds( StartTime,EndTime) },
                    { indexDeletedDurationSeconds, GetDurationSeconds(StartTime, RemovedCaseIndexTime) },
                    { indexSettledDurationSeconds, GetDurationSeconds(RemovedCaseIndexTime, IndexSettledTime) },
                    { getInstancesToTerminateDurationSeconds, GetDurationSeconds(IndexSettledTime, GotTerminateInstancesTime) },
                    { terminateInstancesDurationSeconds, GetDurationSeconds(GotTerminateInstancesTime, TerminatedInstancesTime) },
                    { purgedInstancesDurationSeconds, GetDurationSeconds(TerminatedInstancesTime, EndTime) },
                    { nameof(TerminatedInstancesCount), TerminatedInstancesCount },
                }
            );
        }
    }
}