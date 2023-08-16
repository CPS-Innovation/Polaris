using System;

namespace Common.Telemetry.Wrappers.Contracts
{
    public interface ITelemetryAugmentationWrapper
    {
        void RegisterUserName(string userName);
        void RegisterCorrelationId(Guid correlationId);
    }
}