import * as f from './../functions.bicep'

param project string
param environment string
param location string = resourceGroup().location
param logAnalyticsIdentifier string = 'log'

var logAnalyticsName = '${project}-${environment}-${logAnalyticsIdentifier}-${f.unique4()}'
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
#disable-next-line BCP334
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

