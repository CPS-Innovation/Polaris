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
    //storage_account_name = "cpsdevstorageterraform" //DEV
    storage_account_name = "cpsprodstorageterraform" //PROD
    container_name       = "terraform-networking"
    key                  = "terraform.tfstate"
    access_key           = "[acquire]"
  }*/
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
  resource_name   = format("%s%s", var.resource_name_prefix, "${local.env_name_suffix}")
  common_tags = {
    environment = var.environment.name
    project     = "polaris-${var.resource_name_prefix}"
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

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}
