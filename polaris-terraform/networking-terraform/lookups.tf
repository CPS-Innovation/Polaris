data "azurerm_route_table" "env_route_table" {
  name                = "uks-rt-vnet-innovation-${var.environment.name}"
  resource_group_name = "rg-${var.resource_name_prefix}"
}