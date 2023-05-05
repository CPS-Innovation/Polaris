env                          = "prod"
location                     = "UK South"
environment_tag              = "production"
app_service_plan_web_sku     = "P1v2"
app_service_plan_gateway_sku = "EP1"
app_service_plan_proxy_sku   = "P1v2"
dns_server                   = "10.7.204.164"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

certificate_name    = "polaris.cpsdev.co.uk-polaris-cmsproxy-undefined"
proxy_domain_name_1 = "polaris-cmsproxy.azurewebsites.net"
proxy_domain_name_2 = "polaris.cpsdev.co.uk"

ui_logging = {
  gateway_scale_controller        = "AppInsights:None"
  auth_handover_scale_controller  = "AppInsights:None"
  proxy_scale_controller          = "AppInsights:None"
}