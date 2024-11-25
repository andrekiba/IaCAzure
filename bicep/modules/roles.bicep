param appIdentiy string
param storageAccountName string

resource storageBlobDataContributor 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
}

resource storage 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  scope: resourceGroup()
  name: storageAccountName
}

resource sbdcAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storage
  name: guid(storageAccountName, appIdentiy, storageBlobDataContributor.id)
  properties: {
    roleDefinitionId: storageBlobDataContributor.id
    principalId: appIdentiy
    principalType: 'ServicePrincipal'
  }
}
