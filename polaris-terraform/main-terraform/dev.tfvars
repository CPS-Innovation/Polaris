env             = "dev"
location        = "UK South"
environment_tag = "development"
dns_server      = "10.7.197.20"

ui_component_service_plans = {
  gateway_service_plan_sku        = "EP1"
  gateway_always_ready_instances  = 1
  gateway_maximum_scale_out_limit = 10
  spa_service_plan_sku            = "P1v2"
  proxy_service_plan_sku          = "P1v2"
  maintenance_service_plan_sku    = "B1"
}

pipeline_component_service_plans = {
  coordinator_service_plan_sku           = "P1mv3"
  pdf_generator_service_plan_sku         = "EP2"
  pdf_generator_always_ready_instances   = 1
  pdf_generator_maximum_scale_out_limit  = 10
  pdf_generator_plan_maximum_burst       = 10
  text_extractor_plan_sku                = "EP2"
  text_extractor_always_ready_instances  = 2
  text_extractor_maximum_scale_out_limit = 10
  text_extractor_plan_maximum_burst      = 10
  pdf_redactor_service_plan_sku          = "EP2"
  pdf_redactor_always_ready_instances    = 1
  pdf_redactor_maximum_scale_out_limit   = 10
  pdf_redactor_plan_maximum_burst        = 10
}

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

ui_logging = {
  gateway_scale_controller       = "AppInsights:Verbose"
  auth_handover_scale_controller = "AppInsights:Verbose"
}

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:Verbose"
  pdf_generator_scale_controller  = "AppInsights:Verbose"
  text_extractor_scale_controller = "AppInsights:Verbose"
  pdf_redactor_scale_controller   = "AppInsights:Verbose"
}

cms_details = {
  // for non-prod environments, current thinking is to try to go to Corsham's IP
  //  even if we detect a farnborough cookie
  default_upstream_cms_ip_corsham            = "10.2.177.14"
  default_upstream_cms_modern_ip_corsham     = "10.2.177.67"
  default_upstream_cms_ip_farnborough        = "10.3.177.14"
  default_upstream_cms_modern_ip_farnborough = "10.3.177.67"
  default_upstream_cms_domain_name           = "cin3.cps.gov.uk"
  default_upstream_cms_modern_domain_name    = "cmsmodcin3.cps.gov.uk"
  default_upstream_cms_services_domain_name  = "not-used-in-cin3.cps.gov.uk"
  cin2_upstream_cms_ip_corsham               = "10.2.177.3"
  cin2_upstream_cms_modern_ip_corsham        = "10.2.177.67"
  cin2_upstream_cms_ip_farnborough           = "10.3.177.3"
  cin2_upstream_cms_modern_ip_farnborough    = "10.3.177.67"
  cin2_upstream_cms_domain_name              = "cin2.cps.gov.uk"
  cin2_upstream_cms_modern_domain_name       = "cmsmodcin2.cps.gov.uk"
  cin2_upstream_cms_services_domain_name     = "not-used-in-cin2.cps.gov.uk"
  cin4_upstream_cms_ip_corsham               = "10.2.177.35"
  cin4_upstream_cms_modern_ip_corsham        = "10.2.177.67"
  cin4_upstream_cms_ip_farnborough           = "10.3.177.35"
  cin4_upstream_cms_modern_ip_farnborough    = "10.3.177.67"
  cin4_upstream_cms_domain_name              = "cin4.cps.gov.uk"
  cin4_upstream_cms_modern_domain_name       = "cmsmodstage.cps.gov.uk"
  cin4_upstream_cms_services_domain_name     = "not-used-in-cin4.cps.gov.uk"
  cin5_upstream_cms_ip_corsham               = "10.2.177.21"
  cin5_upstream_cms_modern_ip_corsham        = "10.2.177.67"
  cin5_upstream_cms_ip_farnborough           = "10.3.177.21"
  cin5_upstream_cms_modern_ip_farnborough    = "10.3.177.67"
  cin5_upstream_cms_domain_name              = "cin5.cps.gov.uk"
  cin5_upstream_cms_modern_domain_name       = "cmsmodcin5.cps.gov.uk"
  cin5_upstream_cms_services_domain_name     = "not-used-in-cin5.cps.gov.uk"
}

wm_task_list_host_name  = "https://cps-dev.outsystemsenterprise.com"
auth_handover_whitelist = "/auth-refresh-inbound,https://cps-dev.outsystemsenterprise.com/WorkManagementApp/,https://cps-dev.outsystemsenterprise.com/CaseReview/"

app_service_log_retention       = 90
app_service_log_total_retention = 2555

is_redaction_service_offline = "false"

feature_flag_hte_emails_on = "true"

feature_flag_redaction_log                         = "true"
feature_flag_redaction_log_under_over              = "true"
feature_flag_full_screen                           = "true"
feature_flag_notes                                 = "true"
feature_flag_search_pii                            = "true"
feature_flag_rename_document                       = "true"
feature_flag_reclassify                            = "true"
feature_flag_page_delete                           = "true"
feature_flag_external_redirect_case_review_app     = "true"
feature_flag_external_redirect_bulk_um_app         = "true"
feature_flag_background_pipeline_refresh           = "true"
background_pipeline_refresh_interval_ms            = 0.5 * 60 * 1000
background_pipeline_refresh_show_own_notifications = "true"
feature_flag_page_rotate                           = "true"
local_storage_expiry_days                          = "30"

private_beta = {
  sign_up_url         = "https://forms.office.com/e/Af374akw0Q"
  user_group          = ""
  feature_user_group  = "8fc75d71-3479-4a77-b33b-41fd26ec4960"
  feature_user_group2 = "1663cea9-062e-4f6e-a7ac-26f0942724f3"
  feature_user_group3 = "e9abbdb6-b6e9-4972-90fb-79d3140df840"
  feature_user_group4 = "1e5874e3-1c88-4506-8b9f-4f469acc1a42"
}

case_review_app_redirect_url = "https://cps-dev.outsystemsenterprise.com/CaseReview/Redirect"
bulk_um_redirect_url         = "https://fct-cps-hsk-uifn-dev.azurewebsites.net/api/init"

polaris_ui_reauth = {
  outbound_live_url   = "/polaris,https://cin3.cps.gov.uk/polaris"
  outbound_e2e_url    = "/polaris"
  inbound_url         = "/auth-refresh-inbound"
  use_in_situ_refresh = "true"
  in_situ_termination_url = "/auth-refresh-termination"
}

ssl_certificate_name           = "polaris-dev-notprod3536a9f3-a9a0-48b4-9b40-8c76083cad2e"
ssl_policy_name                = "AppGwSslPolicy20220101"
app_gateway_back_end_host_name = "polaris-dev-notprod.cps.gov.uk"

app_gateway_custom_error_pages = {
  HttpStatus502 = "https://cpsdevstorageterraform.blob.core.windows.net/polaris-error-pages/CaseworkAppUnavailable.html"
  HttpStatus403 = "https://cpsdevstorageterraform.blob.core.windows.net/polaris-error-pages/CaseworkAppUnavailable.html"
}

overnight_clear_down = {
  disabled = 1
  schedule = "0 0 3 * * *"
}

sliding_clear_down = {
  disabled        = 0
  look_back_hours = 12
  protect_blobs   = false
  schedule        = "0 * * * * *"
  batch_size      = 5
}

hte_feature_flag = true

image_conversion_redaction = {
  resolution      = 150
  quality_percent = 50
}

search_service_config = {
  replica_count                 = 3
  partition_count               = 1
  is_dynamic_throttling_enabled = true
}

pii = {
  categories            = "Address;CreditCardNumber;Email;EUDriversLicenseNumber;EUPassportNumber;IPAddress;Person;PhoneNumber;UKDriversLicenseNumber;UKNationalHealthNumber;UKNationalInsuranceNumber;USUKPassportNumber"
  chunk_character_limit = 1000
}

coordinator = {
  control_queue_buffer_threshold        = 256
  max_concurrent_orchestrator_functions = 325
  max_concurrent_activity_functions     = 325
  max_queue_polling_interval            = "00:00:02"
}