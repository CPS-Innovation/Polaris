using System;
using System.Diagnostics;
using PolarisGateway.Domain.Exceptions;

namespace PolarisGateway.Wrappers
{
    public class TelemetryAugmentationWrapper : ITelemetryAugmentationWrapper
    {
        public void AugmentRequestTelemetry(string userName, Guid correlationId)
        {
            Activity activity = Activity.Current;
            if (activity == null)
            {
#if DEBUG
                // MS Issue Activity.Current = null - https://github.com/Azure/azure-functions-host/issues/7651
                // TODO fix for local dev, but ok for now as local dev doesn't need to write to App Insights
                return;
#endif
                throw new CriticalTelemetryException("System.Diagnostics.Activity.Current was expected but found to be null");
            }

            try
            {
                activity.AddTag(TelemetryConstants.UserCustomDimensionName, userName);
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {TelemetryConstants.UserCustomDimensionName}", exception);
            }

            try
            {
                activity.AddTag(TelemetryConstants.CorrelationIdCustomDimensionName, correlationId.ToString());
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {TelemetryConstants.CorrelationIdCustomDimensionName}", exception);
            }
        }
    }
}