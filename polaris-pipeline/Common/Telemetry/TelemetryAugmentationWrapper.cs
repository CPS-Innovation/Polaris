using System;
using System.Diagnostics;

namespace Common.Telemetry
{
    public class TelemetryAugmentationWrapper : ITelemetryAugmentationWrapper
    {
        public void RegisterUserName(string userName)
        {
            RegisterCustomDimension(TelemetryConstants.UserCustomDimensionName, userName);
        }
        public void RegisterCorrelationId(Guid correlationId)
        {

            RegisterCustomDimension(TelemetryConstants.CorrelationIdCustomDimensionName, correlationId.ToString());
        }

        public void RegisterCmsUserId(long cmsUserId)
        {
            RegisterCustomDimension(TelemetryConstants.CmsUserIdCustomDimensionName, cmsUserId.ToString());
        }

        public void RegisterDocumentId(string documentId)
        {
            RegisterCustomDimension(TelemetryConstants.DocumentIdCustomDimensionName, documentId.ToString());
        }

        public void RegisterDocumentVersionId(string documentIdVersionId)
        {
            RegisterCustomDimension(TelemetryConstants.DocumentVersionIdCustomDimensionName, documentIdVersionId);
        }

        public void RegisterClientIp(string clientIp)
        {
            RegisterCustomDimension(TelemetryConstants.ClientIpUnredacted, clientIp);
        }

        public void RegisterLoadBalancingCookie(string loadBalancingCookie)
        {
            RegisterCustomDimension(TelemetryConstants.LoadBalancingCookie, loadBalancingCookie);
        }

        private void RegisterCustomDimension(string key, string value)
        {
            Activity activity = Activity.Current;
            if (activity == null)
            {
#if DEBUG
                // MS Issue Activity.Current = null - https://github.com/Azure/azure-functions-host/issues/7651
                // TODO fix for local dev, but ok for now as local dev doesn't need to write to App Insights
                // Current fix is to downgrade Diagnostics NuGet packages 
#else
                throw new CriticalTelemetryException("System.Diagnostics.Activity.Current was expected but found to be null");
#endif
            }

            try
            {
                activity.AddTag(key, value);
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {key}", exception);
            }
        }
    }
}