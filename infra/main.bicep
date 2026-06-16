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

var storageAccountName = take('${appName}${uniqueString(resourceGroup().id)}', 24)
var appConfigurationName = take('${appName}-cfg-${uniqueString(resourceGroup().id)}', 50)
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
