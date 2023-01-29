data "azuread_application" "fa_pipeline_coordinator" {
  display_name        = "fa-${local.pipeline_resource_name}-coordinator-appreg"
}

data "azuread_application" "fa_pipeline_pdf_generator" {
  display_name        = "fa-${local.pipeline_resource_name}-pdf-generator-appreg"
}

data "azuread_application" "fa_ddei" {
  display_name        = "fa-${local.ddei_resource_name}-appreg"
}

data "azurerm_function_app_host_keys" "fa_pipeline_coordinator_host_keys" {
  name                = "fa-${local.pipeline_resource_name}-coordinator"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_function_app_host_keys" "fa_pipeline_pdf_generator_host_keys" {
  name                = "fa-${local.pipeline_resource_name}-pdf-generator"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azurerm_search_service" "pipeline_ss" {
  name                = "ss-${local.pipeline_resource_name}"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azuread_service_principal" "fa_pipeline_coordinator_service_principal" {
  application_id = data.azuread_application.fa_pipeline_coordinator.application_id
}

data "azuread_service_principal" "fa_pdf_generator_service_principal" {
  application_id = data.azuread_application.fa_pipeline_pdf_generator.application_id
}

data "azuread_service_principal" "fa_ddei_service_principal" {
   application_id = data.azuread_application.fa_ddei.application_id
}