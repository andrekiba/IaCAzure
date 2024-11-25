import * as f from './../functions.bicep'

param project string
param storageIdentifier string = 'st'

@allowed([
  'dev'
  'qa'
  'prod'
])
@description('The target environment for the deployment')
param environment string

@description('The Azure region where resources will be deployed')
param location string = resourceGroup().location

var environmentSettings = {
  dev: {
    skuName: 'Standard_LRS'
  }
  qa: {
    skuName: 'Standard_ZRS'
  }
  prod: {
    skuName: 'Standard_GRS'
  }
}

var storageName = replace('${project}${environment}${storageIdentifier}${f.unique4()}', '-', '')
resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageName
  location: location
  kind: 'StorageV2'
  sku: {
    name: environmentSettings[environment].skuName
  }
}
output storageKey string = storage.listKeys().keys[0].value
output storageName string = storage.name
