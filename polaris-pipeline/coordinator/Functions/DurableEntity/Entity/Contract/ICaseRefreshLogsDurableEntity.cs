using Common.Domain.Entity;
using Common.Dto.Tracker;
using System;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity.Contract
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface ICaseRefreshLogsDurableEntity
    {
        void LogDeltas((DateTime t, CaseDeltasEntity deltas) args);
        void LogCase((DateTime t, CaseRefreshStatus status, string description) args);
        void LogDocument((DateTime t, DocumentLogType status, string polarisDocumentId) args);
        Task<float?> GetMaxTimespan(DocumentLogType status);
    }
}