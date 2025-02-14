import * as f from './../functions.bicep'

param project string
param environment string
param location string = resourceGroup().location
param planIdentifier string = 'plan'
param appIdentifier string = 'app'
@description('Set to true if you want to create a new app service plan')
param createNewPlan bool = true
@description('The existing app service plan id. Mandatory if createNewPlan is set to false')
param existingPlanId string = ''

var environmentSettings = {
  dev: {
    skuName: 'B1'
    skuCapacity: 1
  }
  qa: {
    skuName: 'S1'
    skuCapacity: 2
  }
  prod: {
    skuName: 'P1v2'
    skuCapacity: 2
  }
}

var planName = '${project}-${environment}-${planIdentifier}-${f.unique4()}'
resource plan 'Microsoft.Web/serverfarms@2024-04-01' = if (createNewPlan) {
  name: planName
  location: location
  sku: {
    name: environmentSettings[environment].skuName
    capacity: environmentSettings[environment].skuCapacity
  }
}

/*
resource appManagedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-07-31-preview' = {
  name: '${project}-${environment}-${appIdentifier}-mi'
  location: location
}
*/

var appName = '${project}-${environment}-${appIdentifier}-${f.unique4()}'
resource app 'Microsoft.Web/sites@2024-04-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
    /*
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${appManagedIdentity.id}': {}
    }
    */
  }
  properties: {
    serverFarmId: createNewPlan ? plan.id : existingPlanId
    siteConfig: {
      netFrameworkVersion: 'v9.0'
    }
  }
}

output appUri string = app.properties.defaultHostName
output appName string = app.name
output appIdentity string = app.identity.principalId
