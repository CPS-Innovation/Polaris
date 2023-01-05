variable "display_name" {
  type        = string
  description = "The display name for the application."
}

variable "api" {
  description = "An optional api block, which configures API related settings for this application."
  type        = any
  default     = null
}

variable "app_role" {
  description = "A collection of app_role blocks."
  type        = any
  default     = []
}

variable "device_only_auth_enabled" {
  description = "Specifies whether this application supports device authentication without a user."
  type        = bool
  default     = false
}

variable "fallback_public_client_enabled" {
  description = "Specifies whether the application is a public client. Appropriate for apps using token grant flows that don't use a redirect URI."
  type        = bool
  default     = false
}

variable "group_membership_claims" {
  description = "Configures the groups claim issued in a user or OAuth 2.0 access token that the app expects. Possible values are `None`, `SecurityGroup` or `All`."
  type        = list(string)
  default     = ["SecurityGroup"]
}

variable "identifier_uris" {
  description = "A list of user-defined URI(s) that uniquely identify a Web application within it's Azure AD tenant, or within a verified custom domain if the application is multi-tenant."
  type        = list(string)
  default     = []
}

variable "logo_image" {
  description = " A logo image to upload for the application, as a raw base64-encoded string. The image should be in gif, jpeg or png format. Note that once an image has been uploaded, it is not possible to remove it without replacing it with another image."
  type        = string
  default     = null
}

variable "marketing_url" {
  description = "The URL to the application's home page. If no homepage is specified this defaults to `https://{name}`"
  type        = string
  default     = null
}

variable "oauth2_post_response_required" {
  description = "Specifies whether, as part of OAuth 2.0 token requests, Azure AD allows POST requests, as opposed to GET requests."
  type        = bool
  default     = false
}

variable "optional_claims" {
  description = "An optional claim block."
  type        = any
  default     = null
}

variable "owners" {
  description = "A set of object IDs of principals that will be granted ownership of the application. Supported object types are users or service principals."
  type        = list(string)
  default     = []
}

variable "prevent_duplicate_names" {
  description = "If true, will return an error if an existing application is found with the same name."
  type        = bool
  default     = false
}

variable "privacy_statement_url" {
  description = "URL of the application's privacy statement."
  type        = string
  default     = null
}

variable "public_client" {
  description = "To configure non-web app or non-web API application settings, for example mobile or other public clients such as an installed application running on a desktop device. Must be a valid https or ms-appx-web URL."
  type        = any
  default     = null
}

variable "required_resource_access" {
  description = "A collection of required resource access for this application."
  type        = any
  default     = null
}

variable "sign_in_audience" {
  description = "The Microsoft account types that are supported for the current application. Must be one of `AzureADMyOrg`, `AzureADMultipleOrgs`, `AzureADandPersonalMicrosoftAccount` or `PersonalMicrosoftAccount`."
  type        = string
  default     = "AzureADMyOrg"
}

variable "single_page_application" {
  description = "A single_page_application block, which configures single-page application (SPA) related settings for this application. Must be https."
  type        = any
  default     = null
}

variable "support_url" {
  description = "URL of the application's support page."
  type        = string
  default     = null
}

variable "tags" {
  description = "A set of tags to apply to the application. Cannot be used together with the feature_tags block"
  type        = list(string)
  default     = []
}

variable "template_id" {
  description = "Unique ID for a templated application in the Azure AD App Gallery, from which to create the application."
  type        = string
  default     = null
}

variable "terms_of_service_url" {
  description = "URL of the application's terms of service statement."
  type        = string
  default     = null
}

variable "web" {
  description = "Configures web related settings for this application."
  type        = any
  default     = null
}

