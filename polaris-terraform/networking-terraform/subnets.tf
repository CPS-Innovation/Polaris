resource "azurerm_subnet" "sn_cms_services_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
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

resource "azurerm_subnet_route_table_association" "sn_cms_services_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_cms_services_subnet.id
  depends_on     = [azurerm_subnet.sn_cms_services_subnet]
}

resource "azurerm_subnet" "sn_ddei_services_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-cin-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.ddeiServicesSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

  delegation {
    name = "Microsoft.Web/serverFarms Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_ddei_services_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_ddei_services_subnet.id
  depends_on     = [azurerm_subnet.sn_ddei_services_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_sa_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-pipeline-sa-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineSaSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.CognitiveServices"]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_sa_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_sa_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_sa_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_coordinator_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-pipeline-coordinator-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineCoordinatorSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

  delegation {
    name = "Microsoft.Web/serverFarms Coordinator Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_coordinator_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_coordinator_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_coordinator_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_pdfgenerator_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-pipeline-pdfgenerator-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelinePdfGeneratorSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

  delegation {
    name = "Microsoft.Web/serverFarms PDFGenerator Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_pdfgenerator_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_pdfgenerator_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_pdfgenerator_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_textextractor_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-pipeline-textextractor-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineTextExtractorSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.CognitiveServices"]

  delegation {
    name = "Microsoft.Web/serverFarms TextExtractor Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_textextractor_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_textextractor_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_textextractor_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_keyvault_subnet" {
    #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
    name                 = "polaris-pipeline-keyvault-subnet"
    resource_group_name  = azurerm_resource_group.rg_networking.name
    virtual_network_name = azurerm_virtual_network.vnet_networking.name
    address_prefixes     = [var.polarisPipelineKeyVaultSubnet]
    service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

    enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

    depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_keyvault_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_keyvault_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_keyvault_subnet]
}

resource "azurerm_subnet" "sn_polaris_gateway_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-gateway-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisGatewaySubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

  delegation {
    name = "Microsoft.Web/serverFarms Proxy Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_gateway_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_gateway_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_gateway_subnet]
}

resource "azurerm_subnet" "sn_polaris_ui_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-ui-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisUiSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.Web"]

  delegation {
    name = "Microsoft.Web/serverFarms Proxy Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_ui_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_ui_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_ui_subnet]
}

resource "azurerm_subnet" "sn_polaris_proxy_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-proxy-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisProxySubnet]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "Microsoft.Web/serverFarms Proxy Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_proxy_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_proxy_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_proxy_subnet]
}

resource "azurerm_subnet" "sn_polaris_apps_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-apps-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisAppsSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.CognitiveServices", "Microsoft.Web"]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_apps_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_apps_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_apps_subnet]
}

resource "azurerm_subnet" "sn_polaris_ci_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-ci-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisCiSubnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault", "Microsoft.Web"]

  delegation {
    name = "Microsoft.ContainerInstance/containerGroups Delegation"

    service_delegation {
      name    = "Microsoft.ContainerInstance/containerGroups"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_ci_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_ci_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_ci_subnet]
}

resource "azurerm_subnet" "sn_polaris_dns_resolve_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-dns-resolve-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisDnsResolveSubnet]

  delegation {
    name = "Microsoft.Network/dnsResolvers CnsDns Delegation"

    service_delegation {
      name    = "Microsoft.Network/dnsResolvers"
      actions = ["Microsoft.Network/virtualNetworks/subnets/join/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_gateway_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "GatewaySubnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.gatewaySubnet]

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_auth_handover_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-auth-handover-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisAuthHandoverSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "Microsoft.Web/serverFarms AuthHandover Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet" "sn_polaris_mock_service_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-service-mock-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.mockCmsServiceSubnet]
  service_endpoints    = ["Microsoft.Storage"]

  delegation {
    name = "Microsoft.Web/serverFarms AuthHandover Delegation"

    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_mock_service_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_mock_service_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_mock_service_subnet]
}

resource "azurerm_subnet" "sn_polaris_ampls_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-ampls-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisAmplsSubnet]

  enforce_private_link_endpoint_network_policies = true
  enforce_private_link_service_network_policies  = true

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_ampls_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_ampls_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_ampls_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_netherite_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-pipeline-netherite-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineNetheriteSubnet]
  service_endpoints    = ["Microsoft.EventHub"]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_netherite_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_netherite_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_netherite_subnet]
}

resource "azurerm_subnet" "sn_polaris_pipeline_sa2_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-pipeline-sa2-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisPipelineSa2Subnet]
  service_endpoints    = ["Microsoft.Storage", "Microsoft.KeyVault"]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_pipeline_sa2_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_pipeline_sa2_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_pipeline_sa2_subnet]
}

resource "azurerm_subnet" "sn_polaris_scale_set_subnet" {
  #checkov:skip=CKV2_AZURE_31:Ensure VNET subnet is configured with a Network Security Group (NSG)
  name                 = "polaris-scale-set-subnet"
  resource_group_name  = azurerm_resource_group.rg_networking.name
  virtual_network_name = azurerm_virtual_network.vnet_networking.name
  address_prefixes     = [var.polarisScaleSetSubnet]

  enforce_private_link_endpoint_network_policies = true # DISABLE the policy - setting deprecated in upcoming version 4 of the provider

  depends_on = [azurerm_virtual_network.vnet_networking]
}

resource "azurerm_subnet_route_table_association" "sn_polaris_scale_set_subnet_rt_association" {
  route_table_id = data.azurerm_route_table.env_route_table.id
  subnet_id      = azurerm_subnet.sn_polaris_scale_set_subnet.id
  depends_on     = [azurerm_subnet.sn_polaris_scale_set_subnet]
}