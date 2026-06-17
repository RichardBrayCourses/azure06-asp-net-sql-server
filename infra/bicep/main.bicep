@description('The Azure region where the storage account will be created.')
param location string = resourceGroup().location

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
var sqlServerName = take('${appName}-sql-${uniqueString(resourceGroup().id)}', 63)
var entraAuthority = uri(environment().authentication.loginEndpoint, entraTenantId)

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
  location: location
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
  location: location
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

output storageAccountName string = websiteStorage.name
output appConfigurationName string = appConfiguration.name
output sqlServerName string = sqlServer.name
output sqlServerFullyQualifiedDomainName string = sqlServer.properties.fullyQualifiedDomainName
output sqlDatabaseName string = sqlDatabase.name
output sqlAdministratorLogin string = sqlAdministratorLogin
