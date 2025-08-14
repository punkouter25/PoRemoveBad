targetScope = 'resourceGroup'

// Core parameters
@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

// Computed names
var resourcePrefix = 'rmv'
var resourceToken = uniqueString(subscription().id, resourceGroup().id, location, environmentName)
var appServiceName = 'PoRemoveBad'

// Reference to existing resources in PoShared resource group
var sharedResourceGroupName = 'PoShared'

// Get references to existing resources in PoShared
resource sharedResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = {
  scope: subscription()
  name: sharedResourceGroupName
}

resource existingAppServicePlan 'Microsoft.Web/serverfarms@2024-04-01' existing = {
  scope: sharedResourceGroup
  name: 'PoSharedAppServicePlan'
}

resource existingStorageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  scope: sharedResourceGroup
  name: 'posharedtablestorage'
}

resource existingApplicationInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  scope: sharedResourceGroup
  name: 'PoSharedApplicationInsights'
}

resource existingLogAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2021-06-01' existing = {
  scope: sharedResourceGroup
  name: 'log-iucwaxzqf3hni'
}

// Create user-assigned managed identity
resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: 'az-${resourcePrefix}-identity-${resourceToken}'
  location: location
  tags: {
    'azd-env-name': environmentName
  }
}

// Create the App Service
resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location
  kind: 'app'
  tags: {
    'azd-env-name': environmentName
    'azd-service-name': 'PoRemoveBad'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    publicNetworkAccess: 'Enabled'
    siteConfig: {
      cors: {
        allowedOrigins: ['*']
        supportCredentials: false
      }
      netFrameworkVersion: 'v8.0'
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: existingApplicationInsights.properties.ConnectionString
        }
        {
          name: 'ConnectionStrings:ApplicationInsights'
          value: existingApplicationInsights.properties.ConnectionString
        }
        {
          name: 'ConnectionStrings:AzureTableStorage'
          value: 'DefaultEndpointsProtocol=https;AccountName=${existingStorageAccount.name};AccountKey=${existingStorageAccount.listKeys().keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
        }
        {
          name: 'AZURE_CLIENT_ID'
          value: managedIdentity.properties.clientId
        }
      ]
    }
  }
}

// Note: Role assignments to shared resources in PoShared resource group 
// need to be handled separately after deployment

// Diagnostic settings for the app service
resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  scope: appService
  name: 'az-${resourcePrefix}-diagnostics-${resourceToken}'
  properties: {
    workspaceId: existingLogAnalyticsWorkspace.id
    logs: [
      {
        category: 'AppServiceHTTPLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceConsoleLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
      {
        category: 'AppServiceAppLogs'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: false
          days: 0
        }
      }
    ]
  }
}

// Outputs
output RESOURCE_GROUP_ID string = resourceGroup().id
output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_SUBSCRIPTION_ID string = subscription().subscriptionId

// App Service outputs
output SERVICE_POREMOVERBAD_IDENTITY_PRINCIPAL_ID string = managedIdentity.properties.principalId
output SERVICE_POREMOVERBAD_NAME string = appService.name
output SERVICE_POREMOVERBAD_URI string = 'https://${appService.properties.defaultHostName}'
