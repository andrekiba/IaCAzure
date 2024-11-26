import * as f from './../functions.bicep'

param project string
param environment string
param location string = resourceGroup().location
param sqlServerIdentifier string = 'sql'

//param administratorLogin string = 'adminLogin'
//@secure()
//param administratorLoginPassword string
param administratorManagedIdentityName string
param administratorManagedIdentityId string

var sqlServerName = '${project}-${environment}-${sqlServerIdentifier}-${f.unique4()}'
resource sqlSrv 'Microsoft.Sql/servers@2024-05-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    //administratorLogin: administratorLogin
    //administratorLoginPassword: administratorLoginPassword
    administrators: {
      administratorType: 'ActiveDirectory'
      azureADOnlyAuthentication: true
      login: administratorManagedIdentityName
      sid: administratorManagedIdentityId
      tenantId: subscription().tenantId
    }
  }
}

output sqlServerFqdn string = sqlSrv.properties.fullyQualifiedDomainName
output sqlServerName string = sqlSrv.name
