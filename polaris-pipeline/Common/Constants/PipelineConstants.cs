namespace Common.Constants
{
    public static class HttpHeaderKeys
    {
        public const string ContentType = "Content-Type";
        public const string CorrelationId = "Correlation-Id";
        public const string CmsAuthValues = "Cms-Auth-Values";
    }

    public static class HttpHeaderValues
    {
        public const string ApplicationJson = "application/json";
    }

    public static class EventGridEvents
    {
        public const string BlobDeletedEvent = "Microsoft.Storage.BlobDeleted";
    }

    public static class DocumentTags
    {
        public const string CaseId = "caseId";
        public const string DocumentId = "documentId";
        public const string VersionId = "versionId";
    }

    public static class MiscCategories
    {
        public const string UnknownDocumentType = "1029";
    }

    public static class FeatureFlags
    {
    }

#if DEBUG
    public static class DebugSettings
    {
        public const string UseAzureStorageEmulatorFlag = "POLARIS_AZURE_STORAGE_EMULATOR";
        public const string MockOcrService = "POLARIS_MOCK_OCR_SERVICE";
        public const string MockSearchIndexService = "POLARIS_MOCK_SEARCH_INDEX_SERVICE";
        public const string MockOcrServiceResults = nameof(MockOcrServiceResults);
    }
#endif

    public static class ConfigKeys
    {
        public static class CoordinatorKeys
        {
            public const string CoordinatorOrchestratorTimeoutSecs = "CoordinatorOrchestratorTimeoutSecs";
            public const string PdfGeneratorUrl = "PdfGeneratorUrl";
            public const string TextExtractorUrl = "TextExtractorUrl";
        }

        public static class TextExtractorKeys
        {
            public const string ComputerVisionClientServiceKey = "ComputerVisionClientServiceKey";
            public const string ComputerVisionClientServiceUrl = "ComputerVisionClientServiceUrl";
        }

        public static class SharedKeys
        {
            public const string SearchClientEndpointUrl = "SearchClientEndpointUrl";
            public const string SearchClientIndexName = "SearchClientIndexName";
            public const string SearchClientAuthorizationKey = "SearchClientAuthorizationKey";

            // TODO - resolve these names to a single value
            public const string BlobContainerName = "BlobContainerName";
            public const string BlobServiceContainerName = "BlobServiceContainerName";

            public const string BlobExpirySecs = "BlobExpirySecs";
            public const string BlobUserDelegationKeyExpirySecs = "BlobUserDelegationKeyExpirySecs";
            public const string BlobServiceUrl = nameof(BlobServiceUrl);
            public const string BlobServiceConnectionString = nameof(BlobServiceConnectionString);
            public const string DocumentsRepositoryBaseUrl = "DocumentsRepositoryBaseUrl";
            public const string GetDocumentUrl = "GetDocumentUrl";
            public const string ListDocumentsUrl = "ListDocumentsUrl";
        }
    }
}
