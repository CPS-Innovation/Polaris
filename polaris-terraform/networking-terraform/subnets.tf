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
  
  depends_on = [azurerm_virtual_network.vnet_networking]
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

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_pipeline_sa_subnet" {
  name                 = "polaris-pipeline-sa-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineSaSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_pipeline_coordinator_subnet" {
  name                 = "polaris-pipeline-coordinator-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineCoordinatorSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "Microsoft.Web/serverFarms Coordinator Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_pipeline_pdfgenerator_subnet" {
  name                 = "polaris-pipeline-pdfgenerator-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelinePdfGeneratorSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "Microsoft.Web/serverFarms PDFGenerator Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_pipeline_textextractor_subnet" {
  name                 = "polaris-pipeline-textextractor-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineTextExtractorSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "Microsoft.Web/serverFarms TextExtractor Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_pipeline_keyvault_subnet" {
  name                 = "polaris-pipeline-keyvault-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineKeyVaultSubnet]
  service_endpoints    = ["Microsoft.KeyVault"]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy

  depends_on = [azurerm_virtual_network.vnet_networking]
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

  depends_on = [azurerm_virtual_network.vnet_networking]
}