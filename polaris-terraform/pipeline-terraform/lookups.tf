data "azurerm_function_app_host_keys" "fa_ddei_host_keys" {
  name                = "fa-${local.ddei_resource_name}"
  resource_group_name = "rg-${local.ddei_resource_name}"
}

data "azuread_application" "fa_ddei" {
  display_name        = "fa-${local.ddei_resource_name}-appreg"
}