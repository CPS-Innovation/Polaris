namespace Common.Constants
{
    public static class HttpHeaderKeys
    {
        public const string CorrelationId = "Correlation-Id";
        public const string CmsAuthValues = "Cms-Auth-Values";
        public const string Filetype = nameof(Filetype);
        public const string PolarisDocumentId = nameof(PolarisDocumentId);
        public const string BlobName = nameof(BlobName);
    }

    public static class MiscCategories
    {
        public const string UnknownDocumentType = "1029";
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
