#################### Resource Group ####################
resource "azurerm_resource_group" "rg_rumpole" {
  name     = "rg-${local.resource_name}"
  location = var.location
  tags = {
    environment = var.environment_tag
  }
}
