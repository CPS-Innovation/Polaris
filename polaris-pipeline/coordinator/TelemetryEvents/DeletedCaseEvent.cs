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

        private readonly Guid _correlationId;
        private readonly long _caseId;

        private readonly DateTime _startTime;
        private readonly DateTime _removedCaseIndexTime;
        private readonly DateTime _indexSettledTime;
        private readonly DateTime _gotTerminateInstancesTime;
        private readonly DateTime _terminatedInstancesTime;
        private readonly DateTime _endTime;
        private readonly int _terminatedInstancesCount;

        public DeletedCaseEvent(
            Guid correlationId,
            long caseId,
            DateTime startTime,
            DateTime removedCaseIndexTime,
            DateTime indexSettledTime,
            DateTime gotTerminateInstancesTime,
            DateTime terminatedInstancesTime,
            DateTime endTime,
            int terminatedInstancesCount)
        {
            _correlationId = correlationId;
            _caseId = caseId;
            _startTime = startTime;
            _removedCaseIndexTime = removedCaseIndexTime;
            _indexSettledTime = indexSettledTime;
            _gotTerminateInstancesTime = gotTerminateInstancesTime;
            _terminatedInstancesTime = terminatedInstancesTime;
            _endTime = endTime;
            _terminatedInstancesCount = terminatedInstancesCount;
        }

        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(_correlationId), _correlationId.ToString() },
                    { nameof(_caseId), _caseId.ToString() },
                    { nameof(_startTime), _startTime.ToString("o") },
                    { nameof(_endTime), _endTime.ToString("o") },
                },
                new Dictionary<string, double>
                {
                    { durationSeconds, GetDurationSeconds( _startTime,_endTime) },
                    { indexDeletedDurationSeconds, GetDurationSeconds(_startTime, _removedCaseIndexTime) },
                    { indexSettledDurationSeconds, GetDurationSeconds(_removedCaseIndexTime, _indexSettledTime) },
                    { getInstancesToTerminateDurationSeconds, GetDurationSeconds(_indexSettledTime, _gotTerminateInstancesTime) },
                    { terminateInstancesDurationSeconds, GetDurationSeconds(_gotTerminateInstancesTime, _terminatedInstancesTime) },
                    { purgedInstancesDurationSeconds, GetDurationSeconds(_terminatedInstancesTime, _endTime) },
                    { nameof(_terminatedInstancesCount), _terminatedInstancesCount },
                }
            );
        }
    }
}