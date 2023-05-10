env                          = "dev"
location                     = "UK South"
environment_tag              = "development"
app_service_plan_web_sku     = "P1v2"
app_service_plan_gateway_sku = "EP1"
app_service_plan_proxy_sku   = "P1v2"
dns_server                   = "10.7.197.20"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

certificate_name    = "polaris-dev-notprod3536a9f3-a9a0-48b4-9b40-8c76083cad2e"
proxy_domain_name_1 = "polaris-dev-cmsproxy.azurewebsites.net"
proxy_domain_name_2 = "polaris-dev-notprod.cps.co.uk"

ui_logging = {
  gateway_scale_controller       = "AppInsights:Verbose"
  auth_handover_scale_controller = "AppInsights:Verbose"
  proxy_scale_controller         = "AppInsights:Verbose"
}