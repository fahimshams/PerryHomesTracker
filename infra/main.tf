data "azurerm_resource_group" "main" {
  name = var.resource_group_name
}

locals {
  location = trimspace(var.location) != "" ? var.location : data.azurerm_resource_group.main.location

  environments = {

    staging = {
      sku_name = "B1"
      always_on  = true
    }
    production = {
      sku_name = "B1"
      always_on  = true
    }
  }
}

resource "azurerm_service_plan" "env" {
  for_each = local.environments

  name                = "${var.app_name_prefix}-${each.key}-plan"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = local.location
  os_type             = "Linux"
  sku_name            = each.value.sku_name
}

resource "azurerm_linux_web_app" "env" {
  for_each = local.environments

  name                = "${var.app_name_prefix}-${each.key}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = local.location
  service_plan_id     = azurerm_service_plan.env[each.key].id

  site_config {
    always_on = each.value.always_on
    application_stack {
      dotnet_version = "8.0"
    }
  }
}
