data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azuread_application" "fa_ddei" {
  display_name        = "fa-${local.ddei_resource_name}-appreg"
}

data "azuread_service_principal" "fa_ddei_service_principal" {
  application_id = data.azuread_application.fa_ddei.application_id
}