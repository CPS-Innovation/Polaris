using System;
using System.Diagnostics;
using Common.Constants;
using Common.Domain.Exceptions;
using Common.Telemetry.Wrappers.Contracts;

namespace Common.Telemetry.Wrappers
{
    public class TelemetryAugmentationWrapper : ITelemetryAugmentationWrapper
    {
        public void AddUserName(string userName)
        {
            var activity = GetActivityOrThrow();

            try
            {
                activity.AddTag(TelemetryConstants.UserCustomDimensionName, userName);
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {TelemetryConstants.UserCustomDimensionName}", exception);
            }
        }
        public void AddCorrelationId(Guid correlationId)
        {
            var activity = GetActivityOrThrow();

            try
            {
                activity.AddTag(TelemetryConstants.CorrelationIdCustomDimensionName, correlationId.ToString());
            }
            catch (Exception exception)
            {
                throw new CriticalTelemetryException($"Unable to set {TelemetryConstants.CorrelationIdCustomDimensionName}", exception);
            }
        }

        private static Activity GetActivityOrThrow()
        {
            Activity activity = Activity.Current;
            if (activity == null)
            {
#if DEBUG
                // MS Issue Activity.Current = null - https://github.com/Azure/azure-functions-host/issues/7651
                // TODO fix for local dev, but ok for now as local dev doesn't need to write to App Insights
                // Current fix is to downgrade Diagnostics NuGet packages 
                return new Activity(string.Empty);
#endif
                throw new CriticalTelemetryException("System.Diagnostics.Activity.Current was expected but found to be null");
            }
            return activity;
        }
    }
}