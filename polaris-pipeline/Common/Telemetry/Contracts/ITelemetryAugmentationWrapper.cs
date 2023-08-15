using System;

namespace Common.Telemetry.Wrappers.Contracts
{
    public interface ITelemetryAugmentationWrapper
    {
        void AddUserName(string userName);
        void AddCorrelationId(Guid correlationId);
    }
}