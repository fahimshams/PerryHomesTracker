output "resource_group_name" {
  value       = data.azurerm_resource_group.main.name
  description = "Resource group containing the App Service resources."
}

output "location" {
  value       = local.location
  description = "Region used for App Service resources."
}

output "service_plan_ids" {
  value = {
    for k, plan in azurerm_service_plan.env : k => plan.id
  }
  description = "Map of environment key to Linux App Service plan resource ID."
}

output "linux_web_app_default_hostnames" {
  value = {
    for k, app in azurerm_linux_web_app.env : k => app.default_hostname
  }
  description = "Default *.azurewebsites.net hostnames per environment."
}
