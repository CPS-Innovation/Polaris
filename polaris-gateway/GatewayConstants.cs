namespace PolarisGateway;

public static class ConfigurationKeys
{
    public const string PipelineCoordinatorBaseUrl = "PolarisPipelineCoordinatorBaseUrl";
    public const string PipelineCoordinatorFunctionAppKey = "PolarisPipelineCoordinatorFunctionAppKey";
}

public static class ValidRoles
{
    public const string UserImpersonation = "user_impersonation";
}

public static class CmsAuthConstants
{
    public const string CookieQueryParamName = "cookie";
    public const string PolarisUiQueryParamName = "polaris-ui-url";
    public const string CmsRedirectQueryParamName = "q";
    public const string CmsLaunchModeFallbackRedirectUrl = "/polaris-ui/";
    public const string CmsLaunchModeUiRootUrl = "/polaris-ui/case-details";
}
