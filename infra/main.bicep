targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the environment that can be used as part of naming resource convention')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string

// Optional parameters
@description('Location for OpenAI resource')
param openAiLocation string = 'eastus'

@description('SKU name for OpenAI')
param openAiSkuName string = 'S0'

var abbrs = loadJsonContent('./abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var tags = { 'azd-env-name': environmentName }

// Organize resources in a resource group
resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {
  name: '${abbrs.resourcesResourceGroups}${environmentName}'
  location: location
  tags: tags
}

// App Service Plan
module appServicePlan './core/host/appserviceplan.bicep' = {
  name: 'appserviceplan'
  scope: rg
  params: {
    name: '${abbrs.webServerFarms}${resourceToken}'
    location: location
    tags: tags
    sku: {
      name: 'F1'
      tier: 'Free'
    }
  }
}

// App Service
module web './core/host/appservice.bicep' = {
  name: 'web'
  scope: rg
  params: {
    name: '${abbrs.webSitesAppService}${resourceToken}'
    location: location
    tags: tags
    appServicePlanId: appServicePlan.outputs.id
    runtimeName: 'dotnet'
    runtimeVersion: '9.0'
    appSettings: {
      ASPNETCORE_ENVIRONMENT: 'Production'
    }
  }
}

// Application Insights
module monitoring './core/monitor/monitoring.bicep' = {
  name: 'monitoring'
  scope: rg
  params: {
    location: location
    tags: tags
    logAnalyticsName: '${abbrs.operationalInsightsWorkspaces}${resourceToken}'
    applicationInsightsName: '${abbrs.insightsComponents}${resourceToken}'
  }
}

// Output values
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output APPLICATIONINSIGHTS_CONNECTION_STRING string = monitoring.outputs.applicationInsightsConnectionString
output WEB_URI string = web.outputs.uri
