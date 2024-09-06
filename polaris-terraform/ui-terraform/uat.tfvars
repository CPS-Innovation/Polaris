env             = "uat"
location        = "UK South"
environment_tag = "uat"
dns_server      = "10.7.200.68"

ui_component_service_plans = {
  gateway_service_plan_sku        = "EP1"
  gateway_always_ready_instances  = 1
  gateway_maximum_scale_out_limit = 10
  spa_service_plan_sku            = "P1v2"
  proxy_service_plan_sku          = "P1v2"
  maintenance_service_plan_sku    = "B1"
}

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-uat-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-UAT"

ui_logging = {
  gateway_scale_controller       = "AppInsights:None"
  auth_handover_scale_controller = "AppInsights:None"
}

cms_details = {
  upstream_cms_ip_corsham        = "10.3.177.14"
  upstream_cms_modern_ip_corsham = "10.2.177.67"
  // for non-prod environments, current thinking is to try to go to Corsham's IP
  //  even if we detect a farnborough cookie
  upstream_cms_ip_farnborough        = "10.3.177.14"
  upstream_cms_modern_ip_farnborough = "10.2.177.67"
  upstream_cms_domain_name           = "cin3.cps.gov.uk"
  upstream_cms_modern_domain_name    = "cmsmodcin3.cps.gov.uk"
  upstream_cms_services_domain_name  = "not-used-in-cin3.cps.gov.uk"
}

wm_task_list_host_name = "https://cps-tst.outsystemsenterprise.com"
auth_handover_whitelist = "/auth-refresh-inbound"

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
feature_flag_external_redirect        = "true"
local_storage_expiry_days             = "30"

private_beta = {
  sign_up_url         = "https://forms.office.com/e/Af374akw0Q"
  user_group          = "" // allow any user to see qa for e.g. demo purposes 
  feature_user_group  = "8fc75d71-3479-4a77-b33b-41fd26ec4960"
  feature_user_group2 = "1663cea9-062e-4f6e-a7ac-26f0942724f3"
  feature_user_group3 = "e9abbdb6-b6e9-4972-90fb-79d3140df840"
}

case_review_app_redirect_url   = "https://cps-dev.outsystemsenterprise.com/CaseReview/Redirect"
bulk_um_redirect_url           = "https://cps-dev.outsystemsenterprise.com/CaseReview/Redirect"
polaris_ui_reauth_redirect_url = "/polaris?r=%2Fauth-refresh-inbound%3Fpolaris-ui-url%3D"
