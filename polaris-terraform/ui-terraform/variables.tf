#################### Variables ####################

variable "resource_name_prefix" {
  type    = string
  default = "polaris"
}

variable "networking_resource_name_suffix" {
  default = "networking"
}

variable "env" {
  type = string
}

variable "location" {
  description = "The location of this resource"
  type        = string
}

variable "ui_component_service_plans" {
  type = object({
    gateway_service_plan_sku        = string
    gateway_always_ready_instances  = number
    gateway_maximum_scale_out_limit = number
    spa_service_plan_sku            = string
    proxy_service_plan_sku          = string
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

variable "ui_logging" {
  type = object({
    gateway_scale_controller       = string
    auth_handover_scale_controller = string
  })
}

variable "cms_details" {
  type = object({
    upstream_cms_ip_corsham            = string
    upstream_cms_modern_ip_corsham     = string
    upstream_cms_ip_farnborough        = string
    upstream_cms_modern_ip_farnborough = string
    upstream_cms_domain_name           = string
    upstream_cms_modern_domain_name    = string
    upstream_cms_services_domain_name  = string
  })
}

variable "wm_task_list_host_name" {
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

variable "local_storage_expiry_days" {
  # intentionally a string as this goes in to UI app service's app settings
  type = string
}

variable "private_beta" {
  type = object({
    sign_up_url        = string
    user_group         = string
    feature_user_group = string
  })
}

variable "polaris_ui_reauth_redirect_url" {
  type = string
}