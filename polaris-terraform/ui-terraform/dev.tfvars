env                          = "dev"
location                     = "UK South"
environment_tag              = "Development"
app_service_plan_web_sku     = "P1v2"
app_service_plan_gateway_sku = "EP1"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}