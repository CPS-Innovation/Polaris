#################### Resource Group ####################
resource "azurerm_resource_group" "rg_polaris" {
  name     = "rg-${local.global_resource_name}"
  location = var.location
  tags     = local.common_tags
}

resource "azurerm_resource_group" "rg_polaris_pipeline" {
  name     = "rg-${local.pipeline_resource_name}"
  location = "UK South"
  tags     = local.common_tags
}

resource "azurerm_resource_group" "rg_coordinator" {
  name     = "rg-${var.resource_name_prefix}-coordinator${local.resource_suffix}"
  location = "UK South"
  tags     = local.common_tags
}

resource "azurerm_resource_group" "rg_thumb_gen" {
  name     = "rg-${var.resource_name_prefix}-thumb-gen{local.resource_suffix}"
  location = "UK South"
  tags     = local.common_tags
}
