resource "azurerm_cognitive_account" "computer_vision_service" {
  name                = "cv-${local.resource_name}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  kind                = "ComputerVision"

  sku_name = "S1"
}