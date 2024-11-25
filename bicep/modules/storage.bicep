import * as f from './../functions.bicep'

param project string
param environment string
param location string = resourceGroup().location
param storageIdentifier string = 'st'

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

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storage
  name: 'default'
}

resource testContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'test'
}
//output storageKey string = storage.listKeys().keys[0].value
output storageName string = storage.name
output storageBlobEndpoint string = storage.properties.primaryEndpoints.blob
output storageTestContainerName string = testContainer.name
