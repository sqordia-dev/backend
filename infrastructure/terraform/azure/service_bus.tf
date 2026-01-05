# Service Bus Namespace
resource "azurerm_servicebus_namespace" "main" {
  name                = "${var.project_name}-${var.environment}-servicebus"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = var.service_bus_sku

  tags = merge(
    var.common_tags,
    {
      Name = "${var.project_name}-${var.environment}-sb"
    }
  )
}

# Email Topic
resource "azurerm_servicebus_topic" "email" {
  name         = "${var.project_name}-${var.environment}-email"
  namespace_id = azurerm_servicebus_namespace.main.id

  max_size_in_megabytes = 1024
  default_message_ttl   = "P14D" # 14 days
}

# Email Subscription
resource "azurerm_servicebus_subscription" "email" {
  name     = "${var.project_name}-${var.environment}-email-sub"
  topic_id = azurerm_servicebus_topic.email.id

  max_delivery_count = 10
  default_message_ttl = "P14D"
}

# AI Generation Topic
resource "azurerm_servicebus_topic" "ai_generation" {
  name         = "${var.project_name}-${var.environment}-ai-generation"
  namespace_id = azurerm_servicebus_namespace.main.id

  max_size_in_megabytes = 1024
  default_message_ttl   = "P14D"
}

# AI Generation Subscription
resource "azurerm_servicebus_subscription" "ai_generation" {
  name     = "${var.project_name}-${var.environment}-ai-generation-sub"
  topic_id = azurerm_servicebus_topic.ai_generation.id

  max_delivery_count = 10
  default_message_ttl = "P14D"
}

# Export Topic
resource "azurerm_servicebus_topic" "export" {
  name         = "${var.project_name}-${var.environment}-export"
  namespace_id = azurerm_servicebus_namespace.main.id

  max_size_in_megabytes = 1024
  default_message_ttl   = "P14D"
}

# Export Subscription
resource "azurerm_servicebus_subscription" "export" {
  name     = "${var.project_name}-${var.environment}-export-sub"
  topic_id = azurerm_servicebus_topic.export.id

  max_delivery_count = 10
  default_message_ttl = "P14D"
}

