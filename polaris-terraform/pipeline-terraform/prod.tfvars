env             = "prod"
environment_tag = "production"
dns_server      = "10.7.204.164"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:Verbose"
  pdf_generator_scale_controller  = "AppInsights:Verbose"
  text_extractor_scale_controller = "AppInsights:Verbose"
  pdf_redactor_scale_controller   = "AppInsights:Verbose"
}

pipeline_component_service_plans = {
  coordinator_service_plan_sku           = "EP3"
  coordinator_always_ready_instances     = 2
  coordinator_maximum_scale_out_limit    = 2
  coordinator_plan_maximum_burst         = 10
  pdf_generator_service_plan_sku         = "EP3"
  pdf_generator_always_ready_instances   = 3
  pdf_generator_maximum_scale_out_limit  = 15
  pdf_generator_plan_maximum_burst       = 15
  text_extractor_plan_sku                = "EP3"
  text_extractor_always_ready_instances  = 3
  text_extractor_maximum_scale_out_limit = 10
  text_extractor_plan_maximum_burst      = 10
  pdf_redactor_service_plan_sku          = "EP2"
  pdf_redactor_always_ready_instances    = 3
  pdf_redactor_maximum_scale_out_limit   = 15
  pdf_redactor_plan_maximum_burst        = 15
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

orchestration_switchover = {
  coordinator_switchover_case_id = 1
  coordinator_switchover_modulo = 1
}

coordinator = {
  max_concurrent_orchestrator_functions = 225
  max_concurrent_activity_functions = 225
}