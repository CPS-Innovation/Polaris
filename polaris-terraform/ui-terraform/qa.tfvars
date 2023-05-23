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
custom_domain_name  = "polaris-qa-notprod.cps.gov.uk"

ui_logging = {
  gateway_scale_controller       = "AppInsights:None"
  auth_handover_scale_controller = "AppInsights:None"
  proxy_scale_controller         = "AppInsights:None"
}

cms_details = {
  upstream_cms_ip                 = "10.2.177.14"
  upstream_cms_modern_ip          = "10.2.177.55"
  upstream_cms_domain_name        = "cin3.cps.gov.uk"
  upstream_cms_modern_domain_name = "cmsmodcin3.cps.gov.uk"
}