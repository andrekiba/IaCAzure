targetScope = 'subscription'

param project string
param environment string
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
    administratorLogin: 'adminLogin'
    administratorLoginPassword: 'adminPassword'
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
