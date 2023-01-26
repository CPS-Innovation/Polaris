resource "azurerm_virtual_network" "vnet_networking" {
  name                = "vnet-innovation-${var.environment.name}"
  location            = azurerm_resource_group.rg_networking.location
  resource_group_name = azurerm_resource_group.rg_networking.name
  address_space       = [var.vnetAddressSpace]

  tags = {
    environment = var.environment.name
  }
}

resource "azurerm_subnet" "sn_cms_services_subnet" {
  name                 = "cms-services-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.cmsServicesSubnet]

  delegation {
    name = "Microsoft.Web/serverFarms Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "sn_ddei_services_subnet" {
  name                 = "polaris-cin-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.ddeiServicesSubnet]

  delegation {
    name = "Microsoft.Web/serverFarms Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "sn_polaris_pipeline_sa_subnet" {
  name                 = "polaris-pipeline-sa-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineSaSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  private_endpoint_network_policies_enabled = true
}

resource "azurerm_subnet" "sn_polaris_pipeline_coordinator_subnet" {
  name                 = "polaris-pipeline-coordinator-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineCoordinatorSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  private_endpoint_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms Coordinator Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "sn_polaris_pipeline_pdfgenerator_subnet" {
  name                 = "polaris-pipeline-pdfgenerator-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelinePdfGeneratorSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  private_endpoint_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms PdfGenerator Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "sn_polaris_pipeline_textextractor_subnet" {
  name                 = "polaris-pipeline-textextractor-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineTextExtractorSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  private_endpoint_network_policies_enabled = true

  delegation {
    name = "Microsoft.Web/serverFarms TextExtractor Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

resource "azurerm_subnet" "sn_polaris_ci_subnet" {
  name                 = "polaris-ci-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes      = [var.polarisCiSubnet]
  service_endpoints    = ["Microsoft.Storage"]
  
  delegation {
    name = "Microsoft.ContainerInstance/containerGroups Delegation"

    service_delegation {
      name    = "Microsoft.ContainerInstance/containerGroups"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action", "Microsoft.Network/virtualNetworks/subnets/prepareNetworkPolicies/action"]
    }
  }
}

/*data "azurerm_virtual_hub" "digital_platform_virtual_hub" {
  provider            = azurerm.digital-platform-shared
  name                = "digital-platform-virtual-hub"
  resource_group_name = "digital-platform-virtual-hub"
}

resource "azurerm_virtual_hub_connection" "vhc_innovation" {
  provider                  = azurerm.digital-platform-shared
  name                      = "vnet-innovation-${var.environment.name}"
  virtual_hub_id            = data.azurerm_virtual_hub.digital_platform_virtual_hub.id
  remote_virtual_network_id = azurerm_virtual_network.vnet_networking.id
  internet_security_enabled = true


  routing {
    // This is not ideal. Virtual hub route tables are not currently supported as data attributes. We could do this using Azure CLI in the future as a workaround.
    associated_route_table_id = "/subscriptions/8eeb7cbd-fa86-46be-9112-c72428713fc8/resourceGroups/digital-platform-virtual-hub/providers/Microsoft.Network/virtualHubs/digital-platform-virtual-hub/hubRouteTables/defaultRouteTable"
    propagated_route_table {
      labels          = ["default"]
      route_table_ids = ["/subscriptions/8eeb7cbd-fa86-46be-9112-c72428713fc8/resourceGroups/digital-platform-virtual-hub/providers/Microsoft.Network/virtualHubs/digital-platform-virtual-hub/hubRouteTables/defaultRouteTable"]
    }
  }

  depends_on = [
    azurerm_virtual_network.vnet_networking
  ]
}*/

# Create Private DNS Zones
resource "azurerm_private_dns_zone" "dns_zone_blob_storage" {
  name                = "privatelink.blob.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
}

resource "azurerm_private_dns_zone" "dns_zone_table_storage" {
  name                = "privatelink.table.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
}

resource "azurerm_private_dns_zone" "dns_zone_file_storage" {
  name                = "privatelink.file.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
}

resource "azurerm_private_dns_zone" "dns_zone_apps" {
  name                = "privatelink.azurewebsites.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
}

resource "azurerm_private_dns_zone" "dns_zone_queue_storage" {
  name                = "privatelink.queue.core.windows.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
}

resource "azurerm_private_dns_zone" "dns_zone_key_vault" {
  name                = "privatelink.vaultcore.azure.net"
  resource_group_name = azurerm_resource_group.rg_networking.name
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_blob_storage_link" {
  name                  = "dnszonelink-blobstorage"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_blob_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_table_storage_link" {
  name                  = "dnszonelink-tablestorage"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_table_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_file_storage_link" {
  name                  = "dnszonelink-filestorage"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_file_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_apps_link" {
  name                  = "dnszonelink-apps"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_apps.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_queue_link" {
  name                  = "dnszonelink-queue"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_queue_storage.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id
}

resource "azurerm_private_dns_zone_virtual_network_link" "dns_zone_keyvault_link" {
  name                  = "dnszonelink-keyvault"
  resource_group_name   = azurerm_resource_group.rg_networking.name
  private_dns_zone_name = azurerm_private_dns_zone.dns_zone_key_vault.name
  virtual_network_id    = azurerm_virtual_network.vnet_networking.id
}