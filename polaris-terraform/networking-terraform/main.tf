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

/*
provider "azurerm" {
  alias           = "digital-platform-shared"
  subscription_id = var.digital-platform-shared-subscription-id
  features {}
}
*/

locals {
  env_name_suffix = var.environment.alias != "prod" ? "-${var.environment.alias}" : ""
  env_name        = var.environment.alias != "prod" ? var.environment.alias : ""
  resource_name   = format("%s%s", var.resource_name_prefix, "${local.env_name_suffix}")
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  application_id = "__terraform_service_principal_app_id__"
  //application_id = "ab6f55a4-543f-4f76-bf0a-13bdbd6c324b" // Dev 
  //application_id = "b92f19b6-be30-4292-9763-d4b3340a8a64" // QA
}

data "azurerm_subscription" "current" {}

data "azuread_application_published_app_ids" "well_known" {}

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}
