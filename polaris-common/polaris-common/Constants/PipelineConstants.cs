namespace polaris_common.Constants
{
    public static class HttpHeaderKeys
    {
        public const string ContentType = "Content-Type";
        public const string CorrelationId = "Correlation-Id";
        public const string CmsAuthValues = "Cms-Auth-Values";
        public const string Filetype = nameof(Filetype);
        public const string CaseId = nameof(CaseId);
        public const string DocumentId = nameof(DocumentId);
        public const string VersionId = nameof(VersionId);
        public const string PolarisDocumentId = nameof(PolarisDocumentId);
        public const string BlobName = nameof(BlobName);
        public const string XForwardedFor = "X-Forwarded-For";
    }

    public static class HttpHeaderValues
    {
        public const string ApplicationJson = "application/json";
    }

    public static class OAuthSettings
    {
        public const string TenantId = "TenantId";
        public const string ValidAudience = "CallingAppValidAudience";
        public const string Bearer = "Bearer";
        public const string Authorization = "Authorization";
        //public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
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
        // The Disable flags should default to ON if not present, only required for integration testing
        public const string DisableConvertToPdfFeatureFlag = nameof(DisableConvertToPdfFeatureFlag);
        public const string DisableTextExtractorFeatureFlag = nameof(DisableTextExtractorFeatureFlag);
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

    public static class PipelineSettings
    {
        public const string PipelineCoordinatorBaseUrl = "PolarisPipelineCoordinatorBaseUrl";
        public const string PipelineCoordinatorFunctionAppKey = "PolarisPipelineCoordinatorFunctionAppKey";
        public const string PipelineCoordinatorDurableExtensionCode = "PolarisPipelineCoordinatorDurableExtensionCode";

        public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
        public const string PipelineRedactPdfFunctionAppKey = "PolarisPipelineRedactPdfFunctionAppKey";
        public const string PipelineTextExtractorBaseUrl = "PolarisPipelineTextExtractorBaseUrl";
        public const string PipelineTextExtractorFunctionAppKey = "PolarisPipelineTextExtractorFunctionAppKey";
    }

    public static class ConfigKeys
    {
        public static class CoordinatorKeys
        {
            public const string CoordinatorOrchestratorTimeoutSecs = "CoordinatorOrchestratorTimeoutSecs";
            public const string SlidingClearDownInputDays = nameof(SlidingClearDownInputDays);
            public const string SlidingClearDownProtectBlobs = nameof(SlidingClearDownProtectBlobs);
            public const string SlidingClearDownBatchSize = nameof(SlidingClearDownBatchSize);
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

            public const string BlobServiceContainerName = "BlobServiceContainerName";
            public const string BlobExpirySecs = "BlobExpirySecs";
            public const string BlobUserDelegationKeyExpirySecs = "BlobUserDelegationKeyExpirySecs";
            public const string BlobServiceUrl = nameof(BlobServiceUrl);
        }
    }
}
