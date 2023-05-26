using Common.Dto.Tracker;
using System;

namespace coordinator.Functions.DurableEntity.Entity.Contract
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseRefreshLogsEntity
    {
        void LogDeltas((DateTime t, TrackerDeltasDto deltas) args);
        void LogCase((DateTime t, TrackerLogType status, string description) args);
        void LogDocument((DateTime t, TrackerLogType status, string polarisDocumentId, string description) args);
    }
}