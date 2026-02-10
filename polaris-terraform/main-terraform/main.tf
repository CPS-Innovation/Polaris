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

    restapi = {
      source  = "Mastercard/restapi"
      version = "1.18.2"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.5.1"
    }

    azapi = {
      source  = "Azure/azapi"
      version = "~>2.6.1"
    }
  }

  backend "azurerm" {
  }
}

provider "azurerm" {
  # temporary fix until we can upgrade to >= v3.90, see: https://github.com/hashicorp/terraform-provider-azurerm/issues/27466#issuecomment-2370655250
  skip_provider_registration = true
  features {
    key_vault {
      purge_soft_delete_on_destroy          = true
      purge_soft_deleted_keys_on_destroy    = true
      purge_soft_deleted_secrets_on_destroy = true
      recover_soft_deleted_key_vaults       = true
      recover_soft_deleted_keys             = true
      recover_soft_deleted_secrets          = true
    }
    cognitive_account {
      purge_soft_delete_on_destroy = true
    }
    resource_group {
      prevent_deletion_if_contains_resources = false
    }
  }
}

locals {
  global_resource_name          = var.env != "prod" ? "${var.resource_name_prefix}-${var.env}" : var.resource_name_prefix
  analytics_group_name          = var.env != "prod" ? "${var.resource_name_prefix}-analytics-${var.env}" : "${var.resource_name_prefix}-analytics"
  pipeline_resource_name        = var.env != "prod" ? "${var.resource_name_prefix}-pipeline-${var.env}" : "${var.resource_name_prefix}-pipeline"
  ddei_resource_name            = var.env != "prod" ? "${var.resource_name_prefix}-ddei-${var.env}" : "${var.resource_name_prefix}-ddei"
  mds_resource_name             = var.env != "prod" ? var.env != "dev" ? "eas-app-ddei-staging" : "eas-app-ddei-${var.env}" : "wm-app-ddei"
  materials_resource_name       = var.env != "prod" ? var.env != "dev" ? "as-materials-web-stg-uks" : "as-materials-web-${var.env}-uks" : "as-materials-web-uks"
  mds_mock_resource_name        = var.env != "prod" ? "mock-polaris-ddei-${var.env}" : "mock-polaris-ddei"
  wm_mds_resource_name          = var.env != "prod" ? var.env != "dev" ? "wm-app-ddei-staging" : "wm-app-ddei-${var.env}" : "wm-app-ddei"
  app_service_certificate_store = var.env != "prod" ? "kv-polaris-cert-${var.env}" : "kv-polaris-cert"
  redaction_log_resource_name   = var.env != "prod" ? "redaction-log-${var.env}" : "redaction-log"
  resource_suffix               = var.env != "prod" ? "-${var.env}" : ""
  env_name                      = var.env != "prod" ? var.env : ""

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
  count = 7
}

resource "azuread_service_principal" "msgraph" {
  application_id = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing   = true
}