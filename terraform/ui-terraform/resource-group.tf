#################### Resource Group ####################
resource "azurerm_resource_group" "rg_polaris" {
  name     = "rg-${local.resource_name}"
  location = var.location
  tags = {
    environment = var.environment_tag
  }
}
