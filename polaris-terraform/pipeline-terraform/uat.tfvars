env             = "uat"
environment_tag = "uat"
dns_server      = "10.7.200.68"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-UAT"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:None"
  pdf_generator_scale_controller  = "AppInsights:None"
  text_extractor_scale_controller = "AppInsights:None"
  pdf_redactor_scale_controller   = "AppInsights:None"
}

pipeline_component_service_plans = {
  coordinator_service_plan_sku           = "EP2"
  coordinator_always_ready_instances     = 1
  coordinator_maximum_scale_out_limit    = 2
  coordinator_plan_maximum_burst         = 10
  pdf_generator_service_plan_sku         = "EP2"
  pdf_generator_always_ready_instances   = 1
  pdf_generator_maximum_scale_out_limit  = 10
  pdf_generator_plan_maximum_burst       = 10
  text_extractor_plan_sku                = "EP2"
  text_extractor_always_ready_instances  = 1
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
  max_concurrent_orchestrator_functions = 225
  max_concurrent_activity_functions = 225
  max_queue_polling_interval = "00:00:02"
}