terraform {
  required_version = "1.2.8"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.44.1"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.34.1"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.4.3"
    }

    azapi = {
      source = "Azure/azapi"
    }
  }

  backend "azurerm" {
    storage_account_name = "__terraform_storage_account__"
    container_name       = "__terraform_container_name__"
    key                  = "__terraform_key__"
    access_key           = "__storage_key__"
  }

  /*backend "azurerm" {
    resource_group_name  = "rg-terraform"
    storage_account_name = "cpsdevstorageterraform" //DEV
    //storage_account_name = "cpsqastorageterraform" //QA
    container_name       = "terraform-polaris"
    key                  = "terraform.tfstate"
    access_key           = "[Manually Enter]"
  }*/
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
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  resource_name          = var.env != "prod" ? "${var.resource_name_prefix}-${var.env}" : var.resource_name_prefix
  pipeline_resource_name = var.env != "prod" ? "${var.resource_name_prefix}-pipeline-${var.env}" : "${var.resource_name_prefix}-pipeline"
  ddei_resource_name     = var.env != "prod" ? "${var.resource_name_prefix}-ddei-${var.env}" : "${var.resource_name_prefix}-ddei"
  common_tags = {
    environment = var.environment_tag
    project     = "${var.resource_name_prefix}-ui"
    creator     = "Created by Terraform"
  }
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  application_id = "__terraform_service_principal_app_id__"
  //application_id = "6a2484e4-fd87-41ce-b90d-62f988748192" // Dev 
  //application_id = "db31e9e4-7fa8-4051-ae7b-b283987d3799" // QA
}

data "azurerm_subscription" "current" {}

data "azuread_application_published_app_ids" "well_known" {}

resource "random_uuid" "random_id" {
  count = 1
}

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}