#if SCALABILITY_TEST
using Common.Dto.Document;
using Common.Dto.Tracker;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace coordinator.Functions.DurableEntity.Entity.Contract
{
    // n.b. Entity proxy interface methods must define at most one argument for operation input.
    // (A single tuple is acceptable)
    public interface IScalabilityTestDurableEntity
    {
        void Reset(string TransactionId);
        Task<DateTime> GetStartTime();
        Task<float> GetDurationToCompleted();
        void SetCaseStatus((DateTime T, CaseRefreshStatus Status, string Info) args);
        Task<List<string>> GetCaseDocumentChanges(CmsDocumentDto[] documents);
    }
}
#endif