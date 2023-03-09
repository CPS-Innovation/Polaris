using System;

namespace PolarisGateway.Wrappers
{
    public interface ITelemetryAugmentationWrapper
    {
        void AugmentRequestTelemetry(string userName, Guid correlationId);
    }
}