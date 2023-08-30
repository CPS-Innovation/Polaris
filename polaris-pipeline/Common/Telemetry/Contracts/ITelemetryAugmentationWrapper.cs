using System;

namespace Common.Telemetry.Wrappers.Contracts
{
    public interface ITelemetryAugmentationWrapper
    {
        void RegisterUserName(string userName);
        void RegisterCorrelationId(Guid correlationId);
        void RegisterCmsUserId(long cmsUserId);
        void RegisterDocumentId(string documentId);
        void RegisterDocumentVersionId(string documentIdVersionId);
        void RegisterClientIp(string clientIp);
        void RegisterLoadBalancingCookie(string loadBalancingCookie);
    }
}