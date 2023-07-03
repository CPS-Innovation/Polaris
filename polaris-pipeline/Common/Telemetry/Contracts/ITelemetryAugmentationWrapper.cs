using System;

namespace Common.Telemetry.Wrappers.Contracts
{
    public interface ITelemetryAugmentationWrapper
    {
        void AugmentRequestTelemetry(string userName, Guid correlationId);
    }
}