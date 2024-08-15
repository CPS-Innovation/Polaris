env             = "qa"
location        = "UK South"
environment_tag = "qa"
dns_server      = "10.7.198.164"

ui_component_service_plans = {
  gateway_service_plan_sku        = "EP1"
  gateway_always_ready_instances  = 1
  gateway_maximum_scale_out_limit = 10
  spa_service_plan_sku            = "P1v2"
  proxy_service_plan_sku          = "P1v2"
  maintenance_service_plan_sku    = "B1"
}

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

cms_details = {
  // for non-prod environments, current thinking is to try to go to Corsham's IP
  //  even if we detect a farnborough cookie
  default_upstream_cms_ip_corsham            = "10.3.177.14"
  default_upstream_cms_modern_ip_corsham     = "10.2.177.67"
  default_upstream_cms_ip_farnborough        = "10.3.177.14"
  default_upstream_cms_modern_ip_farnborough = "10.2.177.67"
  default_upstream_cms_domain_name           = "cin3.cps.gov.uk"
  default_upstream_cms_modern_domain_name    = "cmsmodcin3.cps.gov.uk"
  default_upstream_cms_services_domain_name  = "not-used-in-cin3.cps.gov.uk"

  cin2_upstream_cms_ip_corsham            = "10.3.177.3"
  cin2_upstream_cms_modern_ip_corsham     = "10.2.177.67"
  cin2_upstream_cms_ip_farnborough        = "10.3.177.3"
  cin2_upstream_cms_modern_ip_farnborough = "10.2.177.67"
  cin2_upstream_cms_domain_name           = "cin2.cps.gov.uk"
  cin2_upstream_cms_modern_domain_name    = "cmsmodcin2.cps.gov.uk"
  cin2_upstream_cms_services_domain_name  = "not-used-in-cin2.cps.gov.uk"

  cin3_upstream_cms_ip_corsham            = "10.3.177.14"
  cin3_upstream_cms_modern_ip_corsham     = "10.2.177.67"
  cin3_upstream_cms_ip_farnborough        = "10.3.177.14"
  cin3_upstream_cms_modern_ip_farnborough = "10.2.177.67"
  cin3_upstream_cms_domain_name           = "cin3.cps.gov.uk"
  cin3_upstream_cms_modern_domain_name    = "cmsmodcin3.cps.gov.uk"
  cin3_upstream_cms_services_domain_name  = "not-used-in-cin3.cps.gov.uk"

  cin4_upstream_cms_ip_corsham            = "10.3.177.35"
  cin4_upstream_cms_modern_ip_corsham     = "10.2.177.52"
  cin4_upstream_cms_ip_farnborough        = "10.3.177.35"
  cin4_upstream_cms_modern_ip_farnborough = "10.2.177.52"
  cin4_upstream_cms_domain_name           = "cin5.cps.gov.uk"
  cin4_upstream_cms_modern_domain_name    = "cmsmodstage.cps.gov.uk"
  cin4_upstream_cms_services_domain_name  = "not-used-in-cin4.cps.gov.uk"

  cin5_upstream_cms_ip_corsham            = "10.3.177.21"
  cin5_upstream_cms_modern_ip_corsham     = "10.2.177.67"
  cin5_upstream_cms_ip_farnborough        = "10.3.177.21"
  cin5_upstream_cms_modern_ip_farnborough = "10.2.177.67"
  cin5_upstream_cms_domain_name           = "cin5.cps.gov.uk"
  cin5_upstream_cms_modern_domain_name    = "cmsmodcin5.cps.gov.uk"
  cin5_upstream_cms_services_domain_name  = "not-used-in-cin5.cps.gov.uk"

  cmo_upstream_cms_ip_corsham            = "10.3.177.4"
  cmo_upstream_cms_modern_ip_corsham     = "10.2.177.68"
  cmo_upstream_cms_ip_farnborough        = "10.3.177.4"
  cmo_upstream_cms_modern_ip_farnborough = "10.2.177.68"
  cmo_upstream_cms_domain_name           = "cmo.cps.gov.uk"
  cmo_upstream_cms_modern_domain_name    = "cmsmodtrain.cps.gov.uk"
  cmo_upstream_cms_services_domain_name  = "not-used-in-cmo.cps.gov.uk"
}

wm_task_list_host_name = "https://cps-tst.outsystemsenterprise.com"

app_service_log_retention       = 90
app_service_log_total_retention = 2555

is_redaction_service_offline = "false"

feature_flag_hte_emails_on = "true"

feature_flag_redaction_log            = "true"
feature_flag_redaction_log_under_over = "true"
feature_flag_full_screen              = "true"
feature_flag_notes                    = "true"
feature_flag_search_pii               = "true"
feature_flag_rename_document          = "true"
local_storage_expiry_days             = "30"

private_beta = {
  sign_up_url         = "https://forms.office.com/e/Af374akw0Q"
  user_group          = "" // allow any user to see qa for e.g. demo purposes 
  feature_user_group  = "8fc75d71-3479-4a77-b33b-41fd26ec4960"
  feature_user_group2 = "1663cea9-062e-4f6e-a7ac-26f0942724f3"
}

polaris_ui_reauth_redirect_url = "/polaris?polaris-ui-url="

ssl_certificate_name           = "polaris-qa-notprod59598f87-3bda-4304-9ed4-e9c143ee793e"
ssl_policy_name                = "AppGwSslPolicy20220101"
app_gateway_back_end_host_name = "polaris-qa-notprod.cps.gov.uk"

app_gateway_custom_error_pages = {
  HttpStatus502 = "https://cpsqastorageterraform.blob.core.windows.net/polaris-error-pages/CaseworkAppUnavailable.html"
  HttpStatus403 = "https://cpsqastorageterraform.blob.core.windows.net/polaris-error-pages/CaseworkAppUnavailable.html"
}