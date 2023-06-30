using System;
using System.Collections.Generic;
using Common.Telemetry;

namespace coordinator.TelemetryEvents
{
    public class RefreshedCaseEvent : BaseTelemetryEvent
    {
        private readonly Guid _correlationId;
        private readonly long _caseId;
        private readonly int? _versionId;
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly int _cmsDocsCount;
        private readonly int _cmsDocsProcessedCount;
        private readonly int _pcdRequestsProcessedCount;

        public RefreshedCaseEvent(
            Guid correlationId,
            long caseId,
            int? versionId,
            DateTime startTime,
            DateTime endTime,
            int cmsDocsCount,
            int cmsDocsProcessedCount,
            int pcdRequestsProcessedCount)
        {
            _correlationId = correlationId;
            _caseId = caseId;
            _versionId = versionId;
            _startTime = startTime;
            _endTime = endTime;
            _cmsDocsCount = cmsDocsCount;
            _cmsDocsProcessedCount = cmsDocsProcessedCount;
            _pcdRequestsProcessedCount = pcdRequestsProcessedCount;
        }

        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string>
                {
                    { nameof(_correlationId), _correlationId.ToString() },
                    { nameof(_caseId), _caseId.ToString() },
                    { nameof(_versionId), _versionId.ToString() },
                    { nameof(_startTime), _startTime.ToString("o") },
                    { nameof(_endTime), _endTime.ToString("o") },
                },
                new Dictionary<string, double>
                {
                    { durationSeconds, GetDurationSeconds( _startTime,_endTime) },
                    { nameof(_cmsDocsCount), _cmsDocsCount },
                    { nameof(_cmsDocsProcessedCount), _cmsDocsProcessedCount },
                    { nameof(_pcdRequestsProcessedCount), _pcdRequestsProcessedCount },
                }
            );
        }
    }
}