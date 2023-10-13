terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.71.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.38.0"
    }
  }
  required_version = ">= 1.5.3"
}