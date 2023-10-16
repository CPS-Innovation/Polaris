env             = "prod"
environment_tag = "production"
dns_server      = "10.7.204.164"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:Verbose"
  pdf_generator_scale_controller  = "AppInsights:Verbose"
  text_extractor_scale_controller = "AppInsights:Verbose"
}

pipeline_component_service_plans = {
  coordinator_service_plan_sku     = "EP1"
  coordinator_minimum_instances    = 3
  coordinator_maximum_instances    = 10
  pdf_generator_service_plan_sku   = "EP1"
  pdf_generator_minimum_instances  = 3
  pdf_generator_maximum_instances  = 10
  text_extractor_plan_sku          = "EP1"
  text_extractor_minimum_instances = 3
  text_extractor_maximum_instances = 10
}

overnight_clear_down_enabled = false

sliding_clear_down_enabled    = false
sliding_clear_down_input_days = 31
hte_feature_flag              = false