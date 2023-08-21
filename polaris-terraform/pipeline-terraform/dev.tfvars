env                          = "dev"
environment_tag              = "development"
app_service_plan_sku         = "EP1"
coordinator_service_plan_sku = "P1v3"
dns_server                   = "10.7.197.20"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:Verbose"
  pdf_generator_scale_controller  = "AppInsights:Verbose"
  text_extractor_scale_controller = "AppInsights:Verbose"
}

overnight_clear_down_enabled = false

pipeline_event_hub_settings = {
  sku = "Standard"
  capacity = 1
}