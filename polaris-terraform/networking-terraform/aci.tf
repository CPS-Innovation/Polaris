resource "azurerm_storage_account" "aci_storage" {
  name                     = "saacidata${local.env_name}"
  resource_group_name      = azurerm_resource_group.rg_networking.name
  location                 = azurerm_resource_group.rg_networking.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  min_tls_version          = "TLS1_2"

}

resource "azurerm_storage_share" "aci_share" {
  name                 = "polaris-aci-data${local.env_name}"
  storage_account_name = azurerm_storage_account.aci_storage.name
  quota                = 100
}

data "azurerm_resource_group" "rg_containers" {
  name                = "rg-polaris-containers"
}

data "azurerm_container_registry" "acr" {
  name                = "polariscontainers${local.env_name}"
  resource_group_name = data.azurerm_resource_group.rg_containers.name
}

resource "azurerm_container_group" "containergroup_polaris" {
  name                = "acgpolaris${local.env_name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  ip_address_type     = "Private"
  os_type             = "Linux"
  subnet_ids          = [azurerm_subnet.sn_polaris_ci_subnet.id]
  image_registry_credential {
    username = data.azurerm_container_registry.acr.admin_username
    password = data.azurerm_container_registry.acr.admin_password
    server   = data.azurerm_container_registry.acr.login_server
  }
  container {
    name   = "sshcontainer"
    image  = "polariscontainers${local.env_name}.azurecr.io/sshcontainer:latest"
    cpu    = "1.0"
    memory = "2.0"

    ports {
      port     = 8000
      protocol = "TCP"
    }

    environment_variables = {
    }

    volume {
      name                 = "my-volume"
      mount_path           = "/aci-store/"
      storage_account_name = azurerm_storage_account.aci_storage.name
      storage_account_key  = azurerm_storage_account.aci_storage.primary_access_key
      share_name           = azurerm_storage_share.aci_share.name
    }
  }
}