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

variable "app_service_plan_proxy_sku" {
  type = string
}

variable "environment_tag" {
  type        = string
  description = "Environment tag value"
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
    proxy_scale_controller         = string
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

variable "app_service_log_retention" {
  type = number
}

variable "app_service_log_total_retention" {
  type = number
}