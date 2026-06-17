@description('The Azure region where the storage account will be created.')
param location string = resourceGroup().location

@description('The Azure region where Azure SQL will be created.')
param sqlLocation string = location

@description('A short lowercase name used to build the storage account name.')
@minLength(3)
@maxLength(16)
param appName string = 'azure02web'

@description('The deployment environment name.')
param environmentName string

@description('The public domain for this environment.')
param domainName string

@description('The Microsoft Entra application client ID for the UI.')
param entraClientId string

@description('The Microsoft Entra tenant ID used to build the authority URL.')
param entraTenantId string

@description('Optional API scope requested by the UI.')
param entraApiScope string = ''

@description('The Microsoft Entra audience accepted by the API.')
param entraAudience string = ''

@description('Shared demo sign-in key sent by the shell when it loads pre-authentication sign-in options.')
@secure()
param demoSignInKey string = ''

@description('The Azure App Service Plan SKU used by the API host.')
param apiAppServicePlanSku string = 'F1'

@description('The Azure App Configuration SKU.')
@allowed([
  'free'
  'standard'
])
param appConfigurationSku string = 'free'

@description('The Azure SQL administrator login name.')
param sqlAdministratorLogin string = 'allchecksoutadmin'

@description('The Azure SQL administrator password.')
@secure()
param sqlAdministratorPassword string

@description('The Azure SQL database name.')
param sqlDatabaseName string = 'AllChecksOut'

var storageAccountName = take('${appName}${uniqueString(resourceGroup().id)}', 24)
var appConfigurationName = take('${appName}-cfg-${uniqueString(resourceGroup().id)}', 50)
var sqlServerName = take('${appName}-${sqlLocation}-sql-${uniqueString(resourceGroup().id, sqlLocation)}', 63)
var entraAuthority = uri(environment().authentication.loginEndpoint, entraTenantId)
var apiAppServicePlanName = take('${appName}-${environmentName}-api-plan', 60)
var apiAppName = take('${appName}-${environmentName}-api-${uniqueString(resourceGroup().id)}', 60)
var apiBaseUrl = 'https://${apiAppName}.azurewebsites.net'
var frontendOrigin = 'https://${domainName}'
var sqlConnectionString = 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};User ID=${sqlAdministratorLogin};Password=${sqlAdministratorPassword};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
var apiValidAudiences = empty(entraAudience)
  ? union([entraClientId], empty(entraApiScope) ? [] : [replace(entraApiScope, '/access_as_user', '')])
  : union([entraAudience, entraClientId], empty(entraApiScope) ? [] : [replace(entraApiScope, '/access_as_user', '')])

resource websiteStorage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: true
  }
}

resource appConfiguration 'Microsoft.AppConfiguration/configurationStores@2023-03-01' = {
  name: appConfigurationName
  location: location
  sku: {
    name: appConfigurationSku
  }
}

resource sqlServer 'Microsoft.Sql/servers@2023-08-01' = {
  name: sqlServerName
  location: sqlLocation
  properties: {
    administratorLogin: sqlAdministratorLogin
    administratorLoginPassword: sqlAdministratorPassword
    minimalTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    version: '12.0'
  }
}

resource sqlAllowAzureServices 'Microsoft.Sql/servers/firewallRules@2023-08-01' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: sqlLocation
  sku: {
    name: 'GP_S_Gen5'
    tier: 'GeneralPurpose'
    family: 'Gen5'
    capacity: 1
  }
  properties: {
    autoPauseDelay: 60
    minCapacity: json('0.5')
    requestedBackupStorageRedundancy: 'Local'
  }
}

resource apiAppServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: apiAppServicePlanName
  location: location
  sku: {
    name: apiAppServicePlanSku
  }
  kind: 'app'
}

resource casesApiApp 'Microsoft.Web/sites@2023-12-01' = {
  name: apiAppName
  location: location
  kind: 'app'
  dependsOn: [
    sqlDatabase
  ]
  properties: {
    serverFarmId: apiAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Production'
        }
        {
          name: 'ConnectionStrings__AllChecksOut'
          value: sqlConnectionString
        }
        {
          name: 'Cors__AllowedOrigins__0'
          value: frontendOrigin
        }
        {
          name: 'Cors__AllowedOrigins__1'
          value: 'http://localhost:5173'
        }
        {
          name: 'Cors__AllowedOrigins__2'
          value: 'http://127.0.0.1:5173'
        }
        {
          name: 'Authentication__Entra__Authority'
          value: entraAuthority
        }
        {
          name: 'Authentication__Entra__TenantId'
          value: entraTenantId
        }
        {
          name: 'Authentication__Entra__ClientId'
          value: entraClientId
        }
        {
          name: 'Authentication__Entra__Audience'
          value: empty(entraAudience) ? entraClientId : entraAudience
        }
        {
          name: 'Authentication__Entra__ValidAudiences__0'
          value: apiValidAudiences[0]
        }
        {
          name: 'Authentication__Entra__ValidAudiences__1'
          value: length(apiValidAudiences) > 1 ? apiValidAudiences[1] : apiValidAudiences[0]
        }
        {
          name: 'DemoSignIn__Key'
          value: demoSignInKey
        }
      ]
      healthCheckPath: '/health'
    }
  }
}

resource environmentConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'APP_ENVIRONMENT'
  properties: {
    value: environmentName
    contentType: 'text/plain'
  }
}

resource domainConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'APP_DOMAIN_NAME'
  properties: {
    value: domainName
    contentType: 'text/plain'
  }
}

resource entraClientIdConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_ENTRA_CLIENT_ID'
  properties: {
    value: entraClientId
    contentType: 'text/plain'
  }
}

resource entraAuthorityConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_ENTRA_AUTHORITY'
  properties: {
    value: entraAuthority
    contentType: 'text/plain'
  }
}

resource entraApiScopeConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_ENTRA_API_SCOPE'
  properties: {
    value: entraApiScope
    contentType: 'text/plain'
  }
}

resource apiBaseUrlConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_API_BASE_URL'
  properties: {
    value: apiBaseUrl
    contentType: 'text/plain'
  }
}

resource appEnvironmentConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_APP_ENVIRONMENT'
  properties: {
    value: environmentName
    contentType: 'text/plain'
  }
}

resource demoSignInKeyConfig 'Microsoft.AppConfiguration/configurationStores/keyValues@2023-03-01' = {
  parent: appConfiguration
  name: 'VITE_DEMO_SIGN_IN_KEY'
  properties: {
    value: demoSignInKey
    contentType: 'text/plain'
  }
}

output storageAccountName string = websiteStorage.name
output appConfigurationName string = appConfiguration.name
output apiAppName string = casesApiApp.name
output apiBaseUrl string = apiBaseUrl
output sqlServerName string = sqlServer.name
output sqlServerFullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output sqlAdministratorLogin string = sqlAdministratorLogin
