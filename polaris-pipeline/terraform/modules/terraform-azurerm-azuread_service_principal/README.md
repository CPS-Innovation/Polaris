<!-- BEGIN_TF_DOCS -->
## Requirements

| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >=1.0.0 |
| <a name="requirement_azuread"></a> [azuread](#requirement\_azuread) | ~> 2.15.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | ~> 2.80 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azuread"></a> [azuread](#provider\_azuread) | ~> 2.15.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azuread_service_principal.main](https://registry.terraform.io/providers/hashicorp/azuread/latest/docs/resources/service_principal) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_account_enabled"></a> [account\_enabled](#input\_account\_enabled) | Whether or not the service principal account is enabled. | `bool` | `true` | no |
| <a name="input_alternative_names"></a> [alternative\_names](#input\_alternative\_names) | A set of alternative names, used to retrieve service principals by subscription, identify resource group and full resource ids for managed identities. | `list(string)` | `[]` | no |
| <a name="input_app_role_assignment_required"></a> [app\_role\_assignment\_required](#input\_app\_role\_assignment\_required) | Whether this service principal requires an app role assignment to a user or group before Azure AD will issue a user or access token to the application. Defaults to false. | `bool` | `false` | no |
| <a name="input_application_id"></a> [application\_id](#input\_application\_id) | The application ID (client ID) of the application for which to create a service principal. | `string` | n/a | yes |
| <a name="input_description"></a> [description](#input\_description) | A description of the service principal provided for internal end-users. | `string` | `null` | no |
| <a name="input_feature_tags"></a> [feature\_tags](#input\_feature\_tags) | A feature\_tags block as described below. Cannot be used together with the tags property. | `any` | `null` | no |
| <a name="input_login_url"></a> [login\_url](#input\_login\_url) | The URL where the service provider redirects the user to Azure AD to authenticate. Azure AD uses the URL to launch the application from Microsoft 365 or the Azure AD My Apps. When blank, Azure AD performs IdP-initiated sign-on for applications configured with SAML-based single sign-on. | `string` | `null` | no |
| <a name="input_notes"></a> [notes](#input\_notes) | A free text field to capture information about the service principal, typically used for operational purposes. | `string` | `null` | no |
| <a name="input_notification_email_addresses"></a> [notification\_email\_addresses](#input\_notification\_email\_addresses) | A free text field to capture information about the service principal, typically used for operational purposes. | `list(string)` | `[]` | no |
| <a name="input_owners"></a> [owners](#input\_owners) | A set of object IDs of principals that will be granted ownership of the application. Supported object types are users or service principals. | `list(string)` | `[]` | no |
| <a name="input_preferred_single_sign_on_mode"></a> [preferred\_single\_sign\_on\_mode](#input\_preferred\_single\_sign\_on\_mode) | The single sign-on mode configured for this application. Azure AD uses the preferred single sign-on mode to launch the application from Microsoft 365 or the Azure AD My Apps. Supported values are `oidc`, `password`, `saml` or `notSupported`. Omit this property or specify a blank string to unset. | `string` | `null` | no |
| <a name="input_saml_single_sign_on"></a> [saml\_single\_sign\_on](#input\_saml\_single\_sign\_on) | A saml single sign-on block | `any` | `null` | no |
| <a name="input_use_existing"></a> [use\_existing](#input\_use\_existing) | When true, any existing service principal linked to the same application will be automatically imported. When false, an import error will be raised for any pre-existing service principal. | `string` | `null` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_app_role_ids"></a> [app\_role\_ids](#output\_app\_role\_ids) | A mapping of app role values to app role IDs, as published by the associated application, intended to be useful when referencing app roles in other resources in your configuration. |
| <a name="output_app_roles"></a> [app\_roles](#output\_app\_roles) | A list of app roles published by the associated application. |
| <a name="output_application_tenant_id"></a> [application\_tenant\_id](#output\_application\_tenant\_id) | The tenant ID where the associated application is registered. |
| <a name="output_display_name"></a> [display\_name](#output\_display\_name) | The display name of the application associated with this service principal. |
| <a name="output_homepage_url"></a> [homepage\_url](#output\_homepage\_url) | Home page or landing page of the associated application. |
| <a name="output_logout_url"></a> [logout\_url](#output\_logout\_url) | The URL that will be used by Microsoft's authorization service to log out an user using OpenId Connect front-channel, back-channel or SAML logout protocols, taken from the associated application. |
| <a name="output_oauth2_permission_scope_ids"></a> [oauth2\_permission\_scope\_ids](#output\_oauth2\_permission\_scope\_ids) | A mapping of OAuth2.0 permission scope values to scope IDs. |
| <a name="output_oauth2_permission_scopes"></a> [oauth2\_permission\_scopes](#output\_oauth2\_permission\_scopes) | A list of OAuth 2.0 delegated permission scopes exposed by the associated application. |
| <a name="output_object_id"></a> [object\_id](#output\_object\_id) | The object Id of Service principal. |
| <a name="output_redirect_uris"></a> [redirect\_uris](#output\_redirect\_uris) | A list of URLs where user tokens are sent for sign-in with the associated application, or the redirect URIs where OAuth 2.0 authorization codes and access tokens are sent for the associated application. |
| <a name="output_saml_metadata_url"></a> [saml\_metadata\_url](#output\_saml\_metadata\_url) | The URL where the service exposes SAML metadata for federation. |
| <a name="output_service_principal_names"></a> [service\_principal\_names](#output\_service\_principal\_names) | A list of identifier URI(s), copied over from the associated application. |
| <a name="output_sign_in_audience"></a> [sign\_in\_audience](#output\_sign\_in\_audience) | The Microsoft account types that are supported for the associated application. Possible values include AzureADMyOrg, AzureADMultipleOrgs, AzureADandPersonalMicrosoftAccount or PersonalMicrosoftAccount. |
| <a name="output_type"></a> [type](#output\_type) | Identifies whether the service principal represents an application or a managed identity. |
<!-- END_TF_DOCS -->