<!-- BEGIN_TF_DOCS -->

# terraform-azurerm-azuread-app-registration
Azure app registration module that create application registration, scopes and app roles.

Updates in this version:

1. I have created separate module azure_application_password and azure_application_pre_authorized and azuread_service_principal
2. The azuread_application is updated to use lookups and any type variables.

Sample application will create following:

1. App roles as `Query-01.Read` and `Query-01.Write`
2. Oauth2_permission_scope `TestScope_01`
3. Microsoft Graph `User.Read` permission
4. Service principal
5. Password Credentials
6. Identifier Uris ( # To set application uri to api//<app_id>, you need to update via script, this is not possible in terraform)


| Name | Version |
|------|---------|
| <a name="requirement_terraform"></a> [terraform](#requirement\_terraform) | >= 0.13 |
| <a name="requirement_azuread"></a> [azuread](#requirement\_azuread) | >= 2.15.0 |
| <a name="requirement_azurerm"></a> [azurerm](#requirement\_azurerm) | >= 2.59.0 |
| <a name="requirement_random"></a> [random](#requirement\_random) | >= 3.1.0 |

## Providers

| Name | Version |
|------|---------|
| <a name="provider_azuread"></a> [azuread](#provider\_azuread) | >= 2.15.0 |

## Modules

No modules.

## Resources

| Name | Type |
|------|------|
| [azuread_application.main](https://registry.terraform.io/providers/hashicorp/azuread/latest/docs/resources/application) | resource |

## Inputs

| Name | Description | Type | Default | Required |
|------|-------------|------|---------|:--------:|
| <a name="input_api"></a> [api](#input\_api) | An optional api block, which configures API related settings for this application. | `any` | `null` | no |
| <a name="input_app_role"></a> [app\_role](#input\_app\_role) | A collection of app\_role blocks. | `any` | `[]` | no |
| <a name="input_device_only_auth_enabled"></a> [device\_only\_auth\_enabled](#input\_device\_only\_auth\_enabled) | Specifies whether this application supports device authentication without a user. | `bool` | `false` | no |
| <a name="input_display_name"></a> [display\_name](#input\_display\_name) | The display name for the application. | `string` | n/a | yes |
| <a name="input_fallback_public_client_enabled"></a> [fallback\_public\_client\_enabled](#input\_fallback\_public\_client\_enabled) | Specifies whether the application is a public client. Appropriate for apps using token grant flows that don't use a redirect URI. | `bool` | `false` | no |
| <a name="input_group_membership_claims"></a> [group\_membership\_claims](#input\_group\_membership\_claims) | Configures the groups claim issued in a user or OAuth 2.0 access token that the app expects. Possible values are `None`, `SecurityGroup` or `All`. | `list(string)` | <pre>[<br>  "SecurityGroup"<br>]</pre> | no |
| <a name="input_identifier_uris"></a> [identifier\_uris](#input\_identifier\_uris) | A list of user-defined URI(s) that uniquely identify a Web application within it's Azure AD tenant, or within a verified custom domain if the application is multi-tenant. | `list(string)` | `[]` | no |
| <a name="input_logo_image"></a> [logo\_image](#input\_logo\_image) | A logo image to upload for the application, as a raw base64-encoded string. The image should be in gif, jpeg or png format. Note that once an image has been uploaded, it is not possible to remove it without replacing it with another image. | `string` | `null` | no |
| <a name="input_marketing_url"></a> [marketing\_url](#input\_marketing\_url) | The URL to the application's home page. If no homepage is specified this defaults to `https://{name}` | `string` | `null` | no |
| <a name="input_oauth2_post_response_required"></a> [oauth2\_post\_response\_required](#input\_oauth2\_post\_response\_required) | Specifies whether, as part of OAuth 2.0 token requests, Azure AD allows POST requests, as opposed to GET requests. | `bool` | `false` | no |
| <a name="input_optional_claims"></a> [optional\_claims](#input\_optional\_claims) | An optional claim block. | `any` | `null` | no |
| <a name="input_owners"></a> [owners](#input\_owners) | A set of object IDs of principals that will be granted ownership of the application. Supported object types are users or service principals. | `list(string)` | `[]` | no |
| <a name="input_prevent_duplicate_names"></a> [prevent\_duplicate\_names](#input\_prevent\_duplicate\_names) | If true, will return an error if an existing application is found with the same name. | `bool` | `false` | no |
| <a name="input_privacy_statement_url"></a> [privacy\_statement\_url](#input\_privacy\_statement\_url) | URL of the application's privacy statement. | `string` | `null` | no |
| <a name="input_public_client"></a> [public\_client](#input\_public\_client) | To configure non-web app or non-web API application settings, for example mobile or other public clients such as an installed application running on a desktop device. Must be a valid https or ms-appx-web URL. | `any` | `null` | no |
| <a name="input_required_resource_access"></a> [required\_resource\_access](#input\_required\_resource\_access) | A collection of required resource access for this application. | `any` | `null` | no |
| <a name="input_sign_in_audience"></a> [sign\_in\_audience](#input\_sign\_in\_audience) | The Microsoft account types that are supported for the current application. Must be one of `AzureADMyOrg`, `AzureADMultipleOrgs`, `AzureADandPersonalMicrosoftAccount` or `PersonalMicrosoftAccount`. | `string` | `"AzureADMyOrg"` | no |
| <a name="input_single_page_application"></a> [single\_page\_application](#input\_single\_page\_application) | A single\_page\_application block, which configures single-page application (SPA) related settings for this application. Must be https. | `any` | `null` | no |
| <a name="input_support_url"></a> [support\_url](#input\_support\_url) | URL of the application's support page. | `string` | `null` | no |
| <a name="input_tags"></a> [tags](#input\_tags) | A set of tags to apply to the application. Cannot be used together with the feature\_tags block | `list(string)` | `[]` | no |
| <a name="input_template_id"></a> [template\_id](#input\_template\_id) | Unique ID for a templated application in the Azure AD App Gallery, from which to create the application. | `string` | `null` | no |
| <a name="input_terms_of_service_url"></a> [terms\_of\_service\_url](#input\_terms\_of\_service\_url) | URL of the application's terms of service statement. | `string` | `null` | no |
| <a name="input_web"></a> [web](#input\_web) | Configures web related settings for this application. | `any` | `null` | no |

## Outputs

| Name | Description |
|------|-------------|
| <a name="output_app_role_ids"></a> [app\_role\_ids](#output\_app\_role\_ids) | A mapping of app role values to app role IDs, intended to be useful when referencing app roles in other resources in your configuration. |
| <a name="output_client_id"></a> [client\_id](#output\_client\_id) | The application id of AzureAD application created. |
| <a name="output_disabled_by_microsoft"></a> [disabled\_by\_microsoft](#output\_disabled\_by\_microsoft) | Whether Microsoft has disabled the registered application. If the application is disabled, this will be a string indicating the status/reason, e.g. DisabledDueToViolationOfServicesAgreement. |
| <a name="output_logo_url"></a> [logo\_url](#output\_logo\_url) | CDN URL to the application's logo, as uploaded with the logo\_image property. |
| <a name="output_oauth2_permission_scope_ids"></a> [oauth2\_permission\_scope\_ids](#output\_oauth2\_permission\_scope\_ids) | A mapping of OAuth2.0 permission scope values to scope IDs, intended to be useful when referencing permission scopes in other resources in your configuration. |
| <a name="output_object_id"></a> [object\_id](#output\_object\_id) | The object id of application. Can be used to assign roles to user. |
| <a name="output_publisher_domain"></a> [publisher\_domain](#output\_publisher\_domain) | The verified publisher domain for the application. |
<!-- END_TF_DOCS -->