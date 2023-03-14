namespace PolarisGateway;

public static class ConfigurationKeys
{
    public const string ClientId = "ClientId";
    public const string ClientSecret = "ClientSecret";
    public const string BlobServiceUrl = "BlobServiceUrl";
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

public static class TelemetryConstants
{
    public const string UserCustomDimensionName = "User";
    public const string CorrelationIdCustomDimensionName = "PolarisCorrelationId";
}