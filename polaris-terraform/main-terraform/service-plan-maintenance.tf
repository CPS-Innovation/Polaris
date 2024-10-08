#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_maintenance" {
  count = var.env == "dev" ? 1 : 0

  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  #checkov:skip=CKV_AZURE_225:Ensure the App Service Plan is zone redundant
  name                = "asp-${local.global_resource_name}-maintenance"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  os_type             = "Linux"
  sku_name            = var.ui_component_service_plans.maintenance_service_plan_sku

  tags = local.common_tags
}
