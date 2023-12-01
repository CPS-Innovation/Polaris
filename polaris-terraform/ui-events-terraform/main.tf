terraform {
  required_version = ">= 1.5.3"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.80.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.45.0"
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
  features {
    key_vault {
      purge_soft_delete_on_destroy          = false
      purge_soft_deleted_keys_on_destroy    = false
      purge_soft_deleted_secrets_on_destroy = false
      recover_soft_deleted_key_vaults       = true
      recover_soft_deleted_keys             = true
      recover_soft_deleted_secrets          = true
    }
    cognitive_account {
      purge_soft_delete_on_destroy = false
    }
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  pipeline_resource_name = var.env != "prod" ? "${var.pipeline_resource_name_prefix}-${var.env}" : var.pipeline_resource_name_prefix
  gateway_resource_name  = var.env != "prod" ? "${var.polaris_resource_name_prefix}-${var.env}-gateway" : "${var.polaris_resource_name_prefix}-gateway"
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
