terraform {
  required_version = ">= 1.5.3"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.87.0"
    }

    restapi = {
      source  = "Mastercard/restapi"
      version = "1.18.2"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  # temporary fix until we can upgrade to >= v3.90, see: https://github.com/hashicorp/terraform-provider-azurerm/issues/27466#issuecomment-2370655250
  skip_provider_registration = true
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  resource_suffix               = var.env != "prod" ? "-${var.env}" : ""
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  display_name = var.terraform_service_principal_display_name
}

data "azurerm_subscription" "current" {}

data "azuread_application_published_app_ids" "well_known" {}