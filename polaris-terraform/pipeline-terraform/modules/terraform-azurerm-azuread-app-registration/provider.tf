terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.54.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.38.0"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.5.1"
    }
  }
  required_version = "1.4.6"
}