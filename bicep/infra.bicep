targetScope = 'subscription'

@description('The name of the project')
param project string

@allowed([
  'dev'
  'qa'
  'prod'
])
@description('The target environment for the deployment')
param environment string

@description('The Azure region where resources will be deployed')
param location string

var rgName = '${project}-${environment}-rg'
resource rg 'Microsoft.Resources/resourceGroups@2024-07-01' = {
  name: rgName
  location: location
}

module appDeployment './modules/app.bicep' = {
  scope: rg
  name: 'appDeployment'
  params: {
    project: project
    environment: environment
    location: location
  }
}

module storageDeployment './modules/storage.bicep' = {
  scope: rg
  name: 'storageDeployment'
  params: {
    project: project
    environment: environment
    location: location
  }
}

module sqlSrvDeployment './modules/sqlSrv.bicep' = {
  scope: rg
  name: 'sqlSrvDeployment'
  params: {
    project: project
    environment: environment
    location: location
    //administratorLogin: 'adminLogin'
    //administratorLoginPassword: 'adminPassword'
    administratorManagedIdentityName: appDeployment.outputs.appName
    administratorManagedIdentityId: appDeployment.outputs.appIdentity
  }
}

module sqlFwDeployment './modules/sqlFw.bicep' = {
  scope: rg
  name: 'sqlFwDeployment'
  params: {
    existingSqlServeName: sqlSrvDeployment.outputs.sqlServerName
    allowAzure: true
    allowedIpAddresses: ''
  }
}

module sqlDbDeployment './modules/sqlDb.bicep' = {
  scope: rg
  name: 'sqlDbDeployment'
  params: {
    project: project
    environment: environment
    location: location
    existingSqlServeName: sqlSrvDeployment.outputs.sqlServerName
  }
}

module aiDeployment './modules/ai.bicep' = {
  scope: rg
  name: 'aiDeployment'
  params: {
    project: project
    environment: environment
    location: location
  }
}

module rolesDeployment './modules/roles.bicep' = {
  scope: rg
  name: 'rolesDeployment'
  params: {
    appIdentiy: appDeployment.outputs.appIdentity
    storageAccountName: storageDeployment.outputs.storageName
  }
}
