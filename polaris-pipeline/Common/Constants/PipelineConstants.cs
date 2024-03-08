namespace Common.Constants
{
    public static class HttpHeaderKeys
    {
        public const string CorrelationId = "Correlation-Id";
        public const string CmsAuthValues = "Cms-Auth-Values";
        public const string FunctionsKey = "x-functions-key";
    }

    public static class ConfigKeys
    {
        public static class SharedKeys
        {
            public const string BlobServiceContainerName = "BlobServiceContainerName";
            public const string BlobServiceUrl = nameof(BlobServiceUrl);
        }
    }
}
