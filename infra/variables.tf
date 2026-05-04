variable "resource_group_name" {
  type        = string
  description = "Existing resource group that hosts the App Service plans and Linux web apps."
  default     = "mtc-resources"
}

variable "location" {
  type        = string
  description = "Azure region for App Service resources. Leave empty to use the existing resource group's location."
  default     = ""
}

variable "app_name_prefix" {
  type        = string
  description = "Prefix for App Service and plan names (e.g. perry-homes -> perry-homes-dev, perry-homes-dev-plan). App names must be globally unique in Azure."
  default     = "perry-homes"
}
