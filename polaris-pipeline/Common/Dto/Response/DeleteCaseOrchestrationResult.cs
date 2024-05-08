using System;

namespace Common.Dto.Response
{
    public class DeleteCaseOrchestrationResult
    {
        public int TerminatedInstancesCount { get; set; }
        public DateTime GotTerminateInstancesDateTime { get; set; }
        public DateTime TerminatedInstancesTime { get; set; }
        public bool DidOrchestrationsTerminate { get; set; }
        public DateTime TerminatedInstancesSettledDateTime { get; set; }
        public int PurgeInstancesCount { get; set; }
        public int PurgedInstancesCount { get; set;}
        public DateTime GotPurgeInstancesDateTime { get; set; }
        public DateTime OrchestrationEndDateTime { get; set; }
        public bool IsSuccess { get; set; }
        public static DeleteCaseOrchestrationResult Empty() => new DeleteCaseOrchestrationResult
        {
            TerminatedInstancesCount = 0,
            PurgeInstancesCount = 0,
            IsSuccess = false,
        };
    }
}