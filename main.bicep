@description('The name of the application')
param appName string = 'PoRemoveBad'

@description('The location for all resources')
param location string = resourceGroup().location

@description('The environment (dev, prod)')
param environment string = 'dev'

@description('Resource group name for shared resources')
param sharedResourceGroupName string = 'PoShared'

// Reference to the existing shared App Service Plan in PoShared resource group
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-01-01' existing = {
  name: 'PoSharedFree'
  scope: resourceGroup(sharedResourceGroupName)
}

// Reference to the existing Application Insights in PoShared resource group
resource existingAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'PoSharedAppInsights'
  scope: resourceGroup(sharedResourceGroupName)
}

// App Service for the hosted Blazor WebAssembly application
resource appService 'Microsoft.Web/sites@2023-01-01' = {
  name: '${appName}-${environment}'
  location: location
  properties: {
    serverFarmId: existingAppServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: existingAppInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'XDT_MicrosoftApplicationInsights_Mode'
          value: 'Recommended'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
      ]
    }
    httpsOnly: true
    clientAffinityEnabled: false
  }
}

// Azure Table Storage account for the application data
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${toLower(appName)}${environment}data'
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

// Log Analytics Workspace for application-specific logging
resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${appName}-${environment}-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 30
  }
}

// Configure App Service to use the storage account
resource appServiceConfig 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: appService
  name: 'appsettings'
  properties: {
    APPLICATIONINSIGHTS_CONNECTION_STRING: existingAppInsights.properties.ConnectionString
    'ConnectionStrings:AzureTableStorage': 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=${az.environment().suffixes.storage}'
    'ConnectionStrings:ApplicationInsights': existingAppInsights.properties.ConnectionString
    ASPNETCORE_ENVIRONMENT: 'Production'
  }
}

// Outputs for reference
output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output storageAccountName string = storageAccount.name
output applicationInsightsName string = existingAppInsights.name
output logAnalyticsWorkspaceName string = logAnalyticsWorkspace.name
