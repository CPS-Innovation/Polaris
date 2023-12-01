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

    random = {
      source  = "hashicorp/random"
      version = "3.5.1"
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
  analytics_group_name          = var.env != "prod" ? "${var.resource_name_prefix}-analytics-${var.env}" : "${var.resource_name_prefix}-analytics"
  resource_name                 = var.env != "prod" ? "${var.resource_name_prefix}-${var.env}" : var.resource_name_prefix
  pipeline_resource_name        = var.env != "prod" ? "${var.resource_name_prefix}-pipeline-${var.env}" : "${var.resource_name_prefix}-pipeline"
  ddei_resource_name            = var.env != "prod" ? "${var.resource_name_prefix}-ddei-${var.env}" : "${var.resource_name_prefix}-ddei"
  app_service_certificate_store = var.env != "prod" ? "kv-polaris-cert-${var.env}" : "kv-polaris-cert"
  redaction_log_resource_name   = var.env != "prod" ? "redaction-log-${var.env}" : "redaction-log"

  common_tags = {
    environment = var.environment_tag
    project     = "${var.resource_name_prefix}-ui"
    creator     = "Created by Terraform"
  }
}

data "azurerm_client_config" "current" {}

data "azuread_service_principal" "terraform_service_principal" {
  display_name = var.terraform_service_principal_display_name
}

data "azurerm_subscription" "current" {}

data "azuread_application_published_app_ids" "well_known" {}

resource "random_uuid" "random_id" {
  count = 3
}

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}