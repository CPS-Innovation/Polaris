terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.87.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.45.0"
    }
  }
  required_version = ">= 1.5.3"
}