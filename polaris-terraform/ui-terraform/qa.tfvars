env                          = "qa"
location                     = "UK South"
environment_tag              = "qa"
app_service_plan_web_sku     = "P1v2"
app_service_plan_gateway_sku = "EP1"
dns_server                   = "10.7.198.164"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-qa-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}