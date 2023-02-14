using System;
using System.Threading.Tasks;
using PolarisGateway.Domain.PolarisPipeline;

namespace PolarisGateway.Clients.PolarisPipeline
{
    public interface IPipelineClient
    {
        Task TriggerCoordinatorAsync(string caseUrn, int caseId, string cmsAuthValues, bool force, Guid correlationId);

        Task<Tracker> GetTrackerAsync(string caseUrn, int caseId, Guid correlationId);
    }
}

