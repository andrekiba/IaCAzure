import * as f from './../functions.bicep'

param project string
param environment string
param location string = resourceGroup().location
param sqlDbIdentifier string = 'sqldb'
@description('The existing Sql Server Name')
param existingSqlServeName string = ''

var environmentSettings = {
  dev: {
    skuName: 'S0'
    skuCapacity: 1
  }
  qa: {
    skuName: 'S1'
    skuCapacity: 1
  }
  prod: {
    skuName: 'S3'
    skuCapacity: 2
  }
}

resource sqlSrv 'Microsoft.Sql/servers@2024-05-01-preview' existing = {
  name: existingSqlServeName
}

var dbName = '${project}-${environment}-${sqlDbIdentifier}-${f.unique4()}'
resource db 'Microsoft.Sql/servers/databases@2024-05-01-preview' = {
  parent: sqlSrv
  name: dbName
  location: location
  sku: {
    name: environmentSettings[environment].skuName
    capacity: environmentSettings[environment].skuCapacity
  }
  properties:{
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}
