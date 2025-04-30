#################### Variables ####################

variable "blob_service_container_name" {
  type    = string
  default = "documents"
}

variable "blob_thumbnails_container_name" {
  type    = string
  default = "thumbnails"
}

variable "resource_name_prefix" {
  type    = string
  default = "polaris"
}

variable "pipeline_resource_name_prefix" {
  type    = string
  default = "polaris-pipeline"
}

variable "networking_resource_name_suffix" {
  default = "networking"
}

variable "env" {
  type = string
}

variable "search_index_name" {
  type    = string
  default = "lines-index"
}

variable "location" {
  description = "The location of this resource"
  type        = string
}

variable "ui_component_service_plans" {
  type = object({
    gateway_service_plan_sku     = string
    spa_service_plan_sku         = string
    proxy_service_plan_sku       = string
    maintenance_service_plan_sku = string
  })
}

variable "pipeline_component_service_plans" {
  type = object({
    coordinator_service_plan_sku             = string
    pdf_generator_service_plan_sku           = string
    pdf_generator_always_ready_instances     = number
    pdf_generator_maximum_scale_out_limit    = number
    pdf_generator_plan_maximum_burst         = number
    pdf_thumbnail_generator_service_plan_sku = string
    text_extractor_plan_sku                  = string
    text_extractor_always_ready_instances    = number
    text_extractor_maximum_scale_out_limit   = number
    text_extractor_plan_maximum_burst        = number
    pdf_redactor_service_plan_sku            = string
    pdf_redactor_always_ready_instances      = number
    pdf_redactor_maximum_scale_out_limit     = number
    pdf_redactor_plan_maximum_burst          = number
  })
}

variable "environment_tag" {
  type        = string
  description = "Environment tag value"
}

variable "polaris_webapp_details" {
  type = object({
    valid_audience = string
    valid_scopes   = string
    valid_roles    = string
  })
}

variable "dns_server" {
  type = string
}

variable "dns_alt_server" {
  type = string
}

variable "polaris_ui_sub_folder" {
  type = string
  // this value must match the PUBLIC_URL=... value
  //  as seen in the ui project top-level package.json
  //  scripts section.
  default = "polaris-ui"
}

variable "terraform_service_principal_display_name" {
  type = string
}

variable "cms_details" {
  type = object({
    default_upstream_cms_ip_corsham            = string
    default_upstream_cms_modern_ip_corsham     = string
    default_upstream_cms_ip_farnborough        = string
    default_upstream_cms_modern_ip_farnborough = string
    default_upstream_cms_domain_name           = string
    default_upstream_cms_modern_domain_name    = string
    default_upstream_cms_services_domain_name  = string
    cin2_upstream_cms_ip_corsham               = string
    cin2_upstream_cms_modern_ip_corsham        = string
    cin2_upstream_cms_ip_farnborough           = string
    cin2_upstream_cms_modern_ip_farnborough    = string
    cin2_upstream_cms_domain_name              = string
    cin2_upstream_cms_modern_domain_name       = string
    cin2_upstream_cms_services_domain_name     = string
    cin4_upstream_cms_ip_corsham               = string
    cin4_upstream_cms_modern_ip_corsham        = string
    cin4_upstream_cms_ip_farnborough           = string
    cin4_upstream_cms_modern_ip_farnborough    = string
    cin4_upstream_cms_domain_name              = string
    cin4_upstream_cms_modern_domain_name       = string
    cin4_upstream_cms_services_domain_name     = string
    cin5_upstream_cms_ip_corsham               = string
    cin5_upstream_cms_modern_ip_corsham        = string
    cin5_upstream_cms_ip_farnborough           = string
    cin5_upstream_cms_modern_ip_farnborough    = string
    cin5_upstream_cms_domain_name              = string
    cin5_upstream_cms_modern_domain_name       = string
    cin5_upstream_cms_services_domain_name     = string
  })
}

variable "wm_task_list_host_name" {
  type = string
}

variable "auth_handover_whitelist" {
  # Coma-delimited string of URL roots that the proxy will allow auth refresh/handover 
  #  redirects to be forwarded on to.
  #  e.g. "https://foo.bar/,https://baz/buz"
  type = string
}

variable "app_service_log_retention" {
  type = number
}

variable "app_service_log_total_retention" {
  type = number
}

variable "is_redaction_service_offline" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_hte_emails_on" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_redaction_log" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_redaction_log_under_over" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_full_screen" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_notes" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_search_pii" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_rename_document" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_reclassify" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_page_delete" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_page_rotate" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_state_retention"{
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_global_nav"{
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_external_redirect_case_review_app" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_external_redirect_bulk_um_app" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_background_pipeline_refresh" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_redaction_toggle_copy_button" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "feature_flag_document_name_search" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "background_pipeline_refresh_interval_ms" {
  type = number
}

variable "background_pipeline_refresh_show_own_notifications" {
  # For testing it is useful for the tester to be able to drive notifiactions
  #  by editing the case in view and to have notifiactions appear for their
  #  own changes.  Obviously not suitable for prod.
  type = string
}

variable "local_storage_expiry_days" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "private_beta" {
  type = object({
    sign_up_url         = string
    user_group          = string
    feature_user_group  = string
    feature_user_group2 = string
    feature_user_group3 = string
    feature_user_group4 = string
    feature_user_group5 = string
    feature_user_group6 = string
  })
}

variable "polaris_ui_reauth" {
  type = object({
    # Note 1: outbound_live_url takes a comma-delimited list of endpoints where auth could be found for this deployment.
    #  Especially in the "full window" reauth flow (which will hopefully be retired once IE mode to Edge mode cookie copying
    #  is enabled for PROD cms) try to put the most used cms endpoint first and any more exotic environments last
    #  e.g. in QA this setting could be "https://cin3.cps.gov.uk/polaris,/polaris,https://cin4.cps.gov.uk/polaris,https://cin2.cps.gov.uk/polaris,https://cin5.cps.gov.uk/polaris"
    #  We try cin3 first as that is the "main" environment we hook on to.  Then try /polaris which will pick up auth 
    #  from the proxied cin3 cms.  Then add other cin environments to try after that.  Other than being most efficient
    #  the rationale is that if an exotic environment /polaris endpoint is off line then the full page reauth flow
    #  will hang and stall at a broken /polaris endpoint.  So put lesser used ones last to try to avoid ruining the mechanism
    #  for the commonly used environments environments.

    # Notes 2: however in development scenarios where we cannot see/resolve e.g. cin3.cps.gov.uk then the reauth flow will 
    #  hang and break if we put https://cin3.cps.gov.uk/polaris before /polaris. So for pre-prod (at the time of writing)
    #  we should choose to put the locally accessible /polaris first. 
    outbound_live_url       = string
    outbound_e2e_url        = string
    inbound_url             = string
    use_in_situ_refresh     = string
    in_situ_termination_url = string
  })
}

variable "case_review_app_redirect_url" {
  type = string
}

variable "bulk_um_redirect_url" {
  type = string
}

variable "pipeline_logging" {
  type = object({
    pdf_generator_scale_controller  = string
    text_extractor_scale_controller = string
    pdf_redactor_scale_controller   = string
  })
}

variable "overnight_clear_down" {
  type = object({
    disabled = number
    schedule = string
  })
}

variable "sliding_clear_down" {
  type = object({
    disabled        = number
    look_back_hours = number
    protect_blobs   = bool
    schedule        = string
    batch_size      = number
  })
}

variable "thumbnail_generator_sliding_clear_down" {
  type = object({
    disabled    = number
    batch_size  = number
    schedule    = string
    input_hours = number
  })
}

variable "hte_feature_flag" {
  type = bool
}

variable "image_conversion_redaction" {
  type = object({
    resolution      = number
    quality_percent = number
  })
}

variable "search_service_config" {
  type = object({
    replica_count                 = number
    partition_count               = number
    is_dynamic_throttling_enabled = bool
  })
}

variable "pii" {
  type = object({
    categories            = string
    chunk_character_limit = number
  })
}

variable "coordinator" {
  type = object({
    control_queue_buffer_threshold        = number
    max_concurrent_activity_functions     = number
    max_concurrent_orchestrator_functions = number
    max_queue_polling_interval            = string #hh:mm:ss format e.g. "00:00:05" for 5 seconds
  })
}

variable "cps_global_components_url" {
  type = string
}