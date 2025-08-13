@description('The name of the app service')
param name string

@description('The location for the app service')
param location string = resourceGroup().location

@description('The app service plan ID')
param appServicePlanId string

@description('The runtime version')
param runtimeVersion string

@description('The app settings')
param appSettings object = {}

@description('The tags to apply to the app service')
param tags object = {}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    serverFarmId: appServicePlanId
    siteConfig: {
      netFrameworkVersion: runtimeVersion
      appSettings: [for key in items(appSettings): {
        name: key.key
        value: key.value
      }]
      cors: {
        allowedOrigins: [
          '*'
        ]
        supportCredentials: false
      }
    }
    httpsOnly: true
  }
}

output uri string = 'https://${appService.properties.defaultHostName}'
output name string = appService.name
output id string = appService.id
