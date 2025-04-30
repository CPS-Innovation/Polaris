env             = "prod"
location        = "UK South"
environment_tag = "production"
dns_server      = "10.7.204.164"
dns_alt_server  = "168.63.129.16"

ui_component_service_plans = {
  gateway_service_plan_sku     = "P2v3"
  spa_service_plan_sku         = "P1v3"
  proxy_service_plan_sku       = "P1v3"
  maintenance_service_plan_sku = "B1"
}

pipeline_component_service_plans = {
  coordinator_service_plan_sku             = "P3mv3"
  pdf_generator_service_plan_sku           = "EP3"
  pdf_generator_always_ready_instances     = 3
  pdf_generator_maximum_scale_out_limit    = 15
  pdf_generator_plan_maximum_burst         = 15
  pdf_thumbnail_generator_service_plan_sku = "P3mv3"
  text_extractor_plan_sku                  = "EP3"
  text_extractor_always_ready_instances    = 3
  text_extractor_maximum_scale_out_limit   = 10
  text_extractor_plan_maximum_burst        = 10
  pdf_redactor_service_plan_sku            = "EP2"
  pdf_redactor_always_ready_instances      = 3
  pdf_redactor_maximum_scale_out_limit     = 15
  pdf_redactor_plan_maximum_burst          = 15
}

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

pipeline_logging = {
  pdf_generator_scale_controller  = "AppInsights:Verbose"
  text_extractor_scale_controller = "AppInsights:Verbose"
  pdf_redactor_scale_controller   = "AppInsights:Verbose"
}

cms_details = {
  default_upstream_cms_ip_corsham            = "10.2.177.2"
  default_upstream_cms_modern_ip_corsham     = "10.2.177.65"
  default_upstream_cms_ip_farnborough        = "10.3.177.2"
  default_upstream_cms_modern_ip_farnborough = "10.3.177.65"
  default_upstream_cms_domain_name           = "cms.cps.gov.uk"
  default_upstream_cms_modern_domain_name    = "cmsmodern.cps.gov.uk"
  default_upstream_cms_services_domain_name  = "cms-services.cps.gov.uk"
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

wm_task_list_host_name  = "https://cps.outsystemsenterprise.com"
auth_handover_whitelist = "/auth-refresh-inbound,https://cps.outsystemsenterprise.com/WorkManagementApp/,https://cps.outsystemsenterprise.com/CaseReview/,https://housekeeping-fn.cps.gov.uk/"

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
feature_flag_state_retention                       = "true"
feature_flag_global_nav                            = "false"
feature_flag_external_redirect_case_review_app     = "true"
feature_flag_external_redirect_bulk_um_app         = "true"
feature_flag_background_pipeline_refresh           = "true"
feature_flag_redaction_toggle_copy_button          = "false"
feature_flag_document_name_search                  = "false"
background_pipeline_refresh_interval_ms            = 5 * 60 * 1000
background_pipeline_refresh_show_own_notifications = "false"
feature_flag_page_rotate                           = "true"
local_storage_expiry_days                          = "30"

private_beta = {
  sign_up_url         = "https://forms.office.com/e/Af374akw0Q"
  user_group          = ""
  feature_user_group  = "8fc75d71-3479-4a77-b33b-41fd26ec4960"
  feature_user_group2 = "1663cea9-062e-4f6e-a7ac-26f0942724f3"
  feature_user_group3 = "e9abbdb6-b6e9-4972-90fb-79d3140df840"
  feature_user_group4 = "1e5874e3-1c88-4506-8b9f-4f469acc1a42"
  feature_user_group5 = "a5bcc0a5-50e4-49c4-89e0-fad3dced6235"
  feature_user_group6 = "21c21011-b568-4ebb-b013-02d4cd15681a"
}

case_review_app_redirect_url = "https://cps.outsystemsenterprise.com/CaseReview/RedirectCW"
bulk_um_redirect_url         = "https://housekeeping-fn.cps.gov.uk/api/init"

polaris_ui_reauth = {
  outbound_live_url       = "/auth-refresh-outbound,/polaris"
  outbound_e2e_url        = "/polaris"
  inbound_url             = "/auth-refresh-inbound"
  use_in_situ_refresh     = "false"
  in_situ_termination_url = "/auth-refresh-termination"
}

overnight_clear_down = {
  disabled = 1
  schedule = "0 0 3 * * *"
}

sliding_clear_down = {
  disabled = 0
  // 3.5 days to cleardown daytime traffic at night and nightime traffic during day
  look_back_hours = 84
  protect_blobs   = false
  schedule        = "0 * * * * *"
  batch_size      = 3
}

thumbnail_generator_sliding_clear_down = {
  disabled    = 0
  batch_size  = 5
  schedule    = "0 * * * * *"
  input_hours = 12
}

hte_feature_flag = false

image_conversion_redaction = {
  resolution      = 150
  quality_percent = 50
}

search_service_config = {
  replica_count                 = 3
  partition_count               = 4
  is_dynamic_throttling_enabled = true
}

pii = {
  categories            = "Address;CreditCardNumber;Email;EUDriversLicenseNumber;EUPassportNumber;IPAddress;Person;PhoneNumber;UKDriversLicenseNumber;UKNationalHealthNumber;UKNationalInsuranceNumber;USUKPassportNumber"
  chunk_character_limit = 1000
}

coordinator = {
  control_queue_buffer_threshold        = 1000 # not certain if raising this correlates to improved throughput
  max_concurrent_orchestrator_functions = 1000
  max_concurrent_activity_functions     = 1000
  max_queue_polling_interval            = "00:00:02"
}

cps_global_components_url="https://sacpsglobalcomponents.blob.core.windows.net/prod/cps-global-components.js"