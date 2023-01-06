output "object_id" {
  description = "The object id of application. Can be used to assign roles to user."
  value       = azuread_application.main.object_id
}

output "client_id" {
  description = "The application id of AzureAD application created."
  value       = azuread_application.main.application_id
}

output "logo_url" {
  description = "CDN URL to the application's logo, as uploaded with the logo_image property."
  value       = azuread_application.main.logo_url
}

output "disabled_by_microsoft" {
  description = "Whether Microsoft has disabled the registered application. If the application is disabled, this will be a string indicating the status/reason, e.g. DisabledDueToViolationOfServicesAgreement."
  value       = azuread_application.main.disabled_by_microsoft
}

output "publisher_domain" {
  description = "The verified publisher domain for the application."
  value       = azuread_application.main.publisher_domain
}

output "oauth2_permission_scope_ids" {
  description = "A mapping of OAuth2.0 permission scope values to scope IDs, intended to be useful when referencing permission scopes in other resources in your configuration."
  value       = azuread_application.main.oauth2_permission_scope_ids
}

output "app_role_ids" {
  description = "A mapping of app role values to app role IDs, intended to be useful when referencing app roles in other resources in your configuration."
  value       = azuread_application.main.app_role_ids
}
