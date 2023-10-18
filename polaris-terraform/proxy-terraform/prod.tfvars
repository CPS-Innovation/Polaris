env                          = "prod"
environment_tag              = "production"
app_service_plan_proxy_sku   = "P1v2"
dns_server                   = "10.7.204.164"
terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

ui_logging = {
  proxy_scale_controller         = "AppInsights:None"
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
