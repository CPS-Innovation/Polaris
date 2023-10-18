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

terraform_service_principal_display_name = "Azure Pipeline: Innovation-QA"

ui_logging = {
  gateway_scale_controller       = "AppInsights:None"
  auth_handover_scale_controller = "AppInsights:None"
}

app_service_log_retention       = 90
app_service_log_total_retention = 2555

is_redaction_service_offline = "false"

private_beta = {
  sign_up_url = "https://forms.office.com/e/Af374akw0Q"
  user_group  = "" // allow any user to see qa for e.g. demo purposes 
}