terraform {
  required_version = ">= 1.5.3"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.87.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.45.0"
    }

    azapi = {
      source  = "Azure/azapi"
      version = "1.5.0"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  features {
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  env_name_suffix = var.environment.alias != "prod" ? "-${var.environment.alias}" : ""
  env_name        = var.environment.alias != "prod" ? var.environment.alias : ""
  resource_name   = format("%s%s", var.resource_name_prefix, local.env_name_suffix)
  global_name     = var.environment.alias != "prod" ? "${var.app_name_prefix}-${var.environment.alias}" : var.app_name_prefix
  common_tags = {
    environment = var.environment.name
    project     = "polaris-${var.resource_name_prefix}"
    creator     = "Created by Terraform"
  }
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  display_name = var.terraform_service_principal_display_name
}

data "azurerm_subscription" "current" {}

data "azuread_application_published_app_ids" "well_known" {}

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}
