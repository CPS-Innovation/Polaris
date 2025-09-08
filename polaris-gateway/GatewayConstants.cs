﻿namespace PolarisGateway;

public static class ConfigurationKeys
{
    public const string PipelineCoordinatorBaseUrl = "PolarisPipelineCoordinatorBaseUrl";
    public const string PipelineRedactPdfBaseUrl = "PolarisPipelineRedactPdfBaseUrl";
    public const string PdfThumbnailGeneratorBaseUrl = "PolarisPdfThumbnailGeneratorBaseUrl";
    public const string CoordinatorClientTimeoutSeconds = "CoordinatorClientTimeoutSeconds";
    public const string PdfGeneratorClientTimeoutSeconds = "PdfGeneratorClientTimeoutSeconds";
    public const string PdfThumbnailGeneratorClientTimeoutSeconds = "PdfThumbnailGeneratorClientTimeoutSeconds";
}

public static class ValidRoles
{
    public const string UserImpersonation = "user_impersonation";
    public const string ClientCredentials = "AppRole.Polaris.Gateway.Access";
}

public static class CmsAuthConstants
{
    public const string CookieQueryParamName = "cookie";
    public const string PolarisUiQueryParamName = "polaris-ui-url";
    public const string CmsRedirectQueryParamName = "q";
    public const string CmsLaunchModeFallbackRedirectUrl = "/polaris-ui/";
    public const string CmsLaunchModeUiRootUrl = "/polaris-ui/case-details";
}

public static class OAuthSettings
{
    public const string TenantId = "TenantId";
    public const string ValidAudience = "CallingAppValidAudience";
    public const string Bearer = "Bearer";
    public const string Authorization = "Authorization";
    //public const string AzureAuthenticationInstanceUrl = "https://login.microsoftonline.com/";
}
