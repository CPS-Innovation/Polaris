data "azurerm_search_service" "ss" {
  name = "ss-polaris-pipline${local.resource_suffix}"
  resource_group_name = "rg-polaris-pipeline${local.resource_suffix}"
}