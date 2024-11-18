using Common.Telemetry;

namespace pdf_thumbnail_generator.TelemetryEvents
{ 
    public class DeleteCaseThumbnailEvent : BaseTelemetryEvent 
    {
        private const string GetInstancesToTerminateDurationSeconds = nameof(GetInstancesToTerminateDurationSeconds);
        private const string TerminateInstancesDurationSeconds = nameof(TerminateInstancesDurationSeconds);
        private const string TerminatedInstancesSettledDurationSeconds = nameof(TerminatedInstancesSettledDurationSeconds);
        private const string GetInstancesToPurgeDurationSeconds = nameof(GetInstancesToPurgeDurationSeconds);
        private const string PurgedInstancesDurationSeconds = nameof(PurgedInstancesDurationSeconds);
        public Guid CorrelationId;
        public string InstanceId;
        public DateTime StartTime;
        public DateTime BlobsDeletedTime;
        public DateTime GotTerminateInstancesTime;
        public DateTime TerminatedInstancesTime;
        public DateTime TerminatedInstancesSettledTime;
        public DateTime GotPurgeInstancesTime;
        public DateTime EndTime;
        public int TerminatedInstancesCount;
        public bool DidOrchestrationsTerminate;
        public int PurgeInstancesCount;
        public int PurgedInstancesCount;
        
        public DeleteCaseThumbnailEvent(Guid correlationId, string instanceId, DateTime startTime)
        {
            CorrelationId = correlationId;
            InstanceId = instanceId;
            StartTime = startTime;
        }

        public override (IDictionary<string, string>, IDictionary<string, double?>) ToTelemetryEventProps()
        {
            return (
                new Dictionary<string, string> 
                {
                    { nameof(CorrelationId), CorrelationId.ToString() },
                    { nameof(InstanceId), InstanceId },
                    { nameof(StartTime), StartTime.ToString("o") },
                    { nameof(EndTime), EndTime.ToString("o") },
                    { nameof(DidOrchestrationsTerminate), DidOrchestrationsTerminate.ToString() },
                },
                new Dictionary<string, double?>
                {
                    { durationSeconds, GetDurationSeconds( StartTime, EndTime) },
                    { GetInstancesToTerminateDurationSeconds, GetDurationSeconds(BlobsDeletedTime, GotTerminateInstancesTime) },
                    { TerminateInstancesDurationSeconds, GetDurationSeconds(GotTerminateInstancesTime, TerminatedInstancesTime) },
                    { TerminatedInstancesSettledDurationSeconds, GetDurationSeconds(TerminatedInstancesTime, TerminatedInstancesSettledTime) },
                    { GetInstancesToPurgeDurationSeconds, GetDurationSeconds(TerminatedInstancesSettledTime, GotPurgeInstancesTime) },
                    { PurgedInstancesDurationSeconds, GetDurationSeconds(GotPurgeInstancesTime, EndTime) },
                    { nameof(TerminatedInstancesCount), TerminatedInstancesCount },
                    { nameof(PurgeInstancesCount), PurgeInstancesCount },
                    { nameof(PurgedInstancesCount), PurgedInstancesCount },
                });
        }
    }
}