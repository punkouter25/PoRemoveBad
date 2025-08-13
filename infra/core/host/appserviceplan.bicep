@description('The name of the app service plan')
param name string

@description('The location for the app service plan')
param location string = resourceGroup().location

@description('The SKU for the app service plan')
param sku object = {
  name: 'F1'
  tier: 'Free'
}

@description('The tags to apply to the app service plan')
param tags object = {}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: name
  location: location
  tags: tags
  sku: sku
  properties: {
    reserved: false
  }
}

output id string = appServicePlan.id
output name string = appServicePlan.name
