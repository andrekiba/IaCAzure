import * as f from './../functions.bicep'

param project string
param sqlServerIdentifier string = 'sql'

@allowed([
  'dev'
  'qa'
  'prod'
])
@description('The target environment for the deployment')
param environment string

@description('The Azure region where resources will be deployed')
param location string = resourceGroup().location


param administratorLogin string = 'adminLogin'
@secure()
param administratorLoginPassword string

var sqlServerName = '${project}-${environment}-${sqlServerIdentifier}-${f.unique4()}'
resource sqlSrv 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: administratorLogin
    administratorLoginPassword: administratorLoginPassword
  }
}

output sqlServerFqdn string = sqlSrv.properties.fullyQualifiedDomainName
output sqlServerName string = sqlSrv.name
