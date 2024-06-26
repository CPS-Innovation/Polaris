env             = "dev"
environment_tag = "development"
dns_server      = "10.7.197.20"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:Verbose"
  pdf_generator_scale_controller  = "AppInsights:Verbose"
  text_extractor_scale_controller = "AppInsights:Verbose"
  pdf_redactor_scale_controller   = "AppInsights:Verbose"
}

pipeline_component_service_plans = {
  coordinator_service_plan_sku           = "P1mv3"
  pdf_generator_service_plan_sku         = "EP2"
  pdf_generator_always_ready_instances   = 1
  pdf_generator_maximum_scale_out_limit  = 10
  pdf_generator_plan_maximum_burst       = 10
  text_extractor_plan_sku                = "EP2"
  text_extractor_always_ready_instances  = 2
  text_extractor_maximum_scale_out_limit = 10
  text_extractor_plan_maximum_burst      = 10
  pdf_redactor_service_plan_sku          = "EP2"
  pdf_redactor_always_ready_instances    = 1
  pdf_redactor_maximum_scale_out_limit   = 10
  pdf_redactor_plan_maximum_burst        = 10
}

overnight_clear_down = {
  disabled = 1
  schedule = "0 0 3 * * *"
}

sliding_clear_down = {
  disabled        = 0
  look_back_hours = 12
  protect_blobs   = false
  schedule        = "0 * * * * *"
  batch_size      = 5
}

hte_feature_flag = true

image_conversion_redaction = {
  resolution      = 150
  quality_percent = 50
}

search_service_config = {
  replica_count                 = 3
  partition_count               = 1
  is_dynamic_throttling_enabled = true
}

pii = {
  categories            = "Address;CreditCardNumber;Email;EUDriversLicenseNumber;EUPassportNumber;IPAddress;Person;PhoneNumber;UKDriversLicenseNumber;UKNationalHealthNumber;UKNationalInsuranceNumber;USUKPassportNumber"
  chunk_character_limit = 1000
}

coordinator = {
  control_queue_buffer_threshold        = 256
  max_concurrent_orchestrator_functions = 325
  max_concurrent_activity_functions     = 325
  max_queue_polling_interval            = "00:00:02"
}