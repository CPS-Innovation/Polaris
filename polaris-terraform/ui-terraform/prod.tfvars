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

ui_logging = {
  gateway_scale_controller       = "AppInsights:Verbose"
  auth_handover_scale_controller = "AppInsights:Verbose"
}

cms_details = {
  upstream_cms_ip_corsham            = "10.2.177.2"
  upstream_cms_modern_ip_corsham     = "10.2.177.50"
  upstream_cms_ip_farnborough        = "10.3.177.2"
  upstream_cms_modern_ip_farnborough = "10.3.177.50"
  upstream_cms_domain_name           = "cms.cps.gov.uk"
  upstream_cms_modern_domain_name    = "cmsmodern.cps.gov.uk"
  upstream_cms_services_domain_name  = "cms-services.cps.gov.uk"
}

app_service_log_retention       = 90
app_service_log_total_retention = 2555

is_redaction_service_offline = "false"

feature_flag_hte_emails_on = "true"

feature_flag_redaction_log = "true"
feature_flag_redaction_log_over = "false"
redaction_log_user_group = ""

private_beta = {
  sign_up_url = "https://forms.office.com/e/Af374akw0Q"
  user_group  = "4d88565f-227b-4043-995c-038286b79869" // the Polaris-Production Access group
  redaction_log_user_group = "8fc75d71-3479-4a77-b33b-41fd26ec4960"
}
