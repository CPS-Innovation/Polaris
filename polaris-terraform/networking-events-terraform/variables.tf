
#################### Variables ####################

variable "resource_name_prefix" {
  type    = string
  default = "networking"
}

variable "environment" {
  type = object({
    name  = string
    alias = string
  })
}

variable "location" {
  type    = string
  default = "UK South"
}

variable "vnet_address_space" {
  type = string
}

variable "gateway_certificate_details" {
  type = object({
    root_name        = string
    public_cert_data = string
  })
}

variable "vpn_client_sp_id" {
  type = string
}