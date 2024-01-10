namespace polaris_common.Health
{
    /// <summary>
    /// Provide the option to 'shim' in the authentication into the a health check
    /// The API doesn't support per instance injection of parameters 
    /// And as we are using custom security the standard Health check auth wrapper can't be applied
    /// Workaround is to store current Auth and Correlation ID in static methods in the base
    /// These values can be set from the source /health API endpoint, the actual instance of the healthcheck
    /// can't be accessed so funnel it through thread safe static values
    /// </summary>
    public class AuthenticatedHealthCheck
    {
        static public string CmsAuthValue { get; set; }

        static public Guid CorrelationId { get; set;  }

        private static object @lock = new();

        public static void SetAuthValues(string cmsAuthValue, Guid correlationId)
        {
            if(Monitor.TryEnter(@lock))
            {
                CmsAuthValue = cmsAuthValue;
                CorrelationId = correlationId;
            }
            Monitor.Exit(@lock);
        }
    }
}
