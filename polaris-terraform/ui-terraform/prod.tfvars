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

certificate_name    = "polaris.cps.co.uk-polaris-cmsproxy-undefined"
custom_domain_name  = "polaris.cpsdev.co.uk"

ui_logging = {
  gateway_scale_controller       = "AppInsights:None"
  auth_handover_scale_controller = "AppInsights:None"
  proxy_scale_controller         = "AppInsights:None"
}

cms_details = {
  upstream_cms_ip                   = "10.2.177.2"
  upstream_cms_modern_ip            = "10.2.177.50"
  upstream_cms_domain_name          = "cms.cps.gov.uk"
  upstream_cms_modern_domain_name   = "cmsmodern.cps.gov.uk"
  upstream_cms_services_domain_name = "cms-services.cps.gov.uk"
}

react_app_ai_connection_string = "IngestionEndpoint=https://polaris.cps.gov.uk/;LiveEndpoint=https://polaris.cps.gov.uk/"