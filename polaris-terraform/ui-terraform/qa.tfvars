env                          = "qa"
location                     = "UK South"
environment_tag              = "QA"
app_service_plan_web_sku     = "P1V2"
app_service_plan_gateway_sku = "EP1"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-qa-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}