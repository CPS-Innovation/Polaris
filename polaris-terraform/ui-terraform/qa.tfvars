env                          = "qa"
location                     = "UK South"
environment_tag              = "qa"
app_service_plan_web_sku     = "P1v2"
app_service_plan_gateway_sku = "EP1"
app_service_plan_proxy_sku   = "P1v2"
dns_server                   = "10.7.198.164"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-qa-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-QA"

certificate_name    = "polaris-qa-certd0457722-dafa-440f-8d83-0f2cbb1b17ad"
proxy_domain_name_1 = "polaris-qa-cmsproxy.azurewebsites.net"
proxy_domain_name_2 = "polaris-qa-notprod.cpsdev.co.uk"

ui_logging = {
  gateway_scale_controller        = "AppInsights:None"
  auth_handover_scale_controller  = "AppInsights:None"
  proxy_scale_controller          = "AppInsights:None"
}