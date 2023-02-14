namespace PolarisGateway;

public static class ConfigurationKeys
{
    public const string BlobContainerName = "BlobContainerName";
    public const string BlobServiceUrl = "BlobServiceUrl";
    public const string BlobExpirySecs = "BlobExpirySecs";
    public const string BlobUserDelegationKeyExpirySecs = "BlobUserDelegationKeyExpirySecs";
    public const string TenantId = "TenantId";
    public const string ValidAudience = "CallingAppValidAudience";
    public const string PipelineCoordinatorBaseUrl = "PolarisPipelineCoordinatorBaseUrl";
    public const string PipelineCoordinatorFunctionAppKey = "PolarisPipelineCoordinatorFunctionAppKey";
    public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
    public const string PipelineRedactPdfFunctionAppKey = "PolarisPipelineRedactPdfFunctionAppKey";
}

public static class AuthenticationKeys
{
    public const string Authorization = "Authorization";
    public const string Bearer = "Bearer";
}

public static class ValidRoles
{
    public const string UserImpersonation = "user_impersonation";
}

public static class HttpHeaderKeys
{
    public const string CorrelationId = "Correlation-Id";
    public const string CmsAuthValues = "Cms-Auth-Values";
}
