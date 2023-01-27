#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_web" {
  name                = "asp-${local.resource_name}-web"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  os_type             = "Linux"
  sku_name            = var.app_service_plan_web_sku

  tags = {
    environment = var.environment_tag
  }
}
