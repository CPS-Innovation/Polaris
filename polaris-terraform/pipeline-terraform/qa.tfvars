env                          = "qa"
environment_tag              = "qa"
app_service_plan_sku         = "EP1"
coordinator_service_plan_sku = "P1v3"
dns_server                   = "10.7.198.164"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-QA"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:None"
  pdf_generator_scale_controller  = "AppInsights:None"
  text_extractor_scale_controller = "AppInsights:None"
}

overnight_clear_down_enabled = true