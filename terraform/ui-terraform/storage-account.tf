resource "azurerm_storage_account" "sacpspolaris" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polaris"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location

  account_kind              = "StorageV2"
  account_replication_type  = "RAGRS"
  account_tier              = "Standard"
  enable_https_traffic_only = true
  min_tls_version           = "TLS1_2"

  network_rules {
    default_action = "Allow"
  }
  tags = {
    environment = var.environment_tag
  }
}
