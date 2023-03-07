terraform {
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
  required_version = "1.2.8"
}