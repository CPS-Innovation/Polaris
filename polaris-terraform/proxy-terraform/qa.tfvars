env                          = "qa"
environment_tag              = "qa"
app_service_plan_proxy_sku   = "P1v2"
dns_server                   = "10.7.198.164"
terraform_service_principal_display_name = "Azure Pipeline: Innovation-QA"

ui_logging = {
  proxy_scale_controller         = "AppInsights:None"
}

cms_details = {
  upstream_cms_ip_corsham        = "10.2.177.14"
  upstream_cms_modern_ip_corsham = "10.2.177.55"
  // for non-prod environments, current thinking is to try to go to Corsham's IP
  //  even if we dtect a farnborough cookie
  upstream_cms_ip_farnborough        = "10.2.177.14"
  upstream_cms_modern_ip_farnborough = "10.2.177.55"
  upstream_cms_domain_name           = "cin3.cps.gov.uk"
  upstream_cms_modern_domain_name    = "cmsmodcin3.cps.gov.uk"
  upstream_cms_services_domain_name  = "not-used-in-cin3.cps.gov.uk"

}

app_service_log_retention       = 90
app_service_log_total_retention = 2555