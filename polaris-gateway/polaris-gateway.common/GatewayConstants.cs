namespace PolarisGateway;

public static class ConfigurationKeys
{
    public const string TenantId = "TenantId";
    public const string ClientId = "ClientId";
    public const string ClientSecret = "ClientSecret";
    public const string ValidAudience = "CallingAppValidAudience";
    public const string PipelineCoordinatorBaseUrl = "PolarisPipelineCoordinatorBaseUrl";
    public const string PipelineCoordinatorFunctionAppKey = "PolarisPipelineCoordinatorFunctionAppKey";
    public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
    public const string PipelineRedactPdfFunctionAppKey = "PolarisPipelineRedactPdfFunctionAppKey";
    public const string BlobServiceUrl = "BlobServiceUrl";
}

public static class AuthenticationKeys
{
    public const string Authorization = "Authorization";
    public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
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

public static class CmsAuthConstants
{
    public const string CookieQueryParamName = "cookie";
    public const string PolarisUiQueryParamName = "polaris-ui-url";
}