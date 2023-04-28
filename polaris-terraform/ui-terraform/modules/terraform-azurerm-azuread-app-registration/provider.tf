terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.53.0"
    }

    azuread = {
      source  = "hashicorp/azuread"
      version = "2.37.2"
    }

    random = {
      source  = "hashicorp/random"
      version = "3.4.3"
    }
  }
  required_version = "1.4.6"
}