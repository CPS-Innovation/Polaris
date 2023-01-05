﻿namespace Common.Constants
{
    public static class AuthenticationKeys
    {
        public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
        public const string AzureAuthenticationAssertionType = "urn:ietf:params:oauth:grant-type:jwt-bearer";
        public const string Bearer = "Bearer";
    }

    public static class HttpHeaderKeys
    {
        public const string Authorization = "Authorization";
        public const string ContentType = "Content-Type";
        public const string CorrelationId = "Correlation-Id";
        public const string UpstreamTokenName = "upstream-token";
    }

    public static class HttpHeaderValues
    {
        public const string ApplicationJson = "application/json";
        public const string AuthTokenType = "Bearer";
    }

    public static class EventGridEvents
    {
        public const string BlobDeletedEvent = "Microsoft.Storage.BlobDeleted";
    }

    public static class PipelineRoles
    {
        public const string EmptyRole = "";
        public const string ExtractText = "application.extracttext";
    }

    public static class PipelineScopes
    {
        public const string GeneratePdf = "user_impersonation";
        public const string RedactPdf = "user_impersonation";
        public const string ProcessEvaluatedDocuments = "user_impersonation";
        public const string EmptyScope = "";
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

    public static class ConfigKeys
    {
        public static class CoordinatorKeys
        {
            public const string CoordinatorOrchestratorTimeoutSecs = "CoordinatorOrchestratorTimeoutSecs";
            public const string OnBehalfOfTokenTenantId = "OnBehalfOfTokenTenantId";
            public const string OnBehalfOfTokenClientId = "OnBehalfOfTokenClientId";
            public const string OnBehalfOfTokenClientSecret = "OnBehalfOfTokenClientSecret";
            public const string PdfGeneratorScope = "PdfGeneratorScope";
            public const string PdfGeneratorUrl = "PdfGeneratorUrl";
            public const string TextExtractorScope = "TextExtractorScope";
            public const string TextExtractorUrl = "TextExtractorUrl";
            public const string DocumentEvaluatorScope = "DocumentEvaluatorScope";
            public const string DocumentEvaluatorUrl = "DocumentEvaluatorUrl";
        }

        public static class TextExtractorKeys
        {
            public const string ComputerVisionClientServiceKey = "ComputerVisionClientServiceKey";
            public const string ComputerVisionClientServiceUrl = "ComputerVisionClientServiceUrl";
        }

        public static class SharedKeys
        {
            public const string CallingAppTenantId = "CallingAppTenantId";
            public const string CallingAppValidAudience = "CallingAppValidAudience";
            public const string SearchClientEndpointUrl = "SearchClientEndpointUrl";
            public const string SearchClientIndexName = "SearchClientIndexName";
            public const string SearchClientAuthorizationKey = "SearchClientAuthorizationKey";
            public const string BlobServiceContainerName = "BlobServiceContainerName";
            public const string BlobExpirySecs = "BlobExpirySecs";
            public const string BlobUserDelegationKeyExpirySecs = "BlobUserDelegationKeyExpirySecs";
            public const string BlobServiceUrl = "BlobServiceUrl";
            public const string DocumentsRepositoryBaseUrl = "DocumentsRepositoryBaseUrl";
            public const string GetDocumentUrl = "GetDocumentUrl";
            public const string ListDocumentsUrl = "ListDocumentsUrl";
            public const string DocumentEvaluatorQueueUrl = "DocumentEvaluatorQueueUrl";
            public const string UpdateBlobStorageQueueName = "UpdateBlobStorageQueueName";
            public const string UpdateSearchIndexByVersionQueueName = "UpdateSearchIndexByVersionQueueName";
            public const string UpdateSearchIndexByBlobNameQueueName = "UpdateSearchIndexByBlobNameQueueName";
        }
    }
}
