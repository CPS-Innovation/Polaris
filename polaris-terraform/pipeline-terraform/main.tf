terraform {

  required_version = ">=1.0.0"
  
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.30.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 2.15.0"
    }

    restapi = {
      source = "Mastercard/restapi"
      version = "1.16.1"
    }

    random = {
      source  = "hashicorp/random"
      version = "~> 3.1.0"
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
    //storage_account_name = "cpsqastorageterraform" //QA
    storage_account_name = "cpsdevstorageterraform" //DEV
    container_name       = "terraform-polaris-pipeline"
    key                  = "terraform.tfstate"
    access_key           = "[Manually Acquired]"
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
    cognitive_account {
      purge_soft_delete_on_destroy = false
    }
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  resource_name = var.env != "prod" ? "${var.resource_name_prefix}-${var.env}" : var.resource_name_prefix
  ddei_resource_name = var.env != "prod" ? "${var.ddei_resource_name_prefix}-${var.env}" : var.ddei_resource_name_prefix
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  application_id = "__terraform_service_principal_app_id__"
  //application_id = "ab6f55a4-543f-4f76-bf0a-13bdbd6c324b" // Dev 
  //application_id = "b92f19b6-be30-4292-9763-d4b3340a8a64" // QA
}

data "azurerm_subscription" "current" {}

resource "random_uuid" "random_id" {
  count = 4
}

data "azuread_application_published_app_ids" "well_known" {}

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}
