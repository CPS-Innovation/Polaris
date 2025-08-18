terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.87.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.53.1"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.5.1"
    }
  }
  required_version = ">= 1.5.3"
}