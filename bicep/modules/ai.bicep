import * as f from './../functions.bicep'

param project string
param environment string
param location string = resourceGroup().location
param applicationInsightsIdentifier string = 'ai'
param logAnalyticsIdentifier string = 'log'
@description('Set to true if you want to create a new log analytics')
param createNewLogAnalytics bool = true
@description('The existing log analytics id. Mandatory if createNewLogAnalytics is set to false')
param existingLogAnalyticsId string = ''

var newLogAnalyticsName = '${project}-${environment}-${logAnalyticsIdentifier}-${f.unique4()}'
resource newLogAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = if (createNewLogAnalytics) {
#disable-next-line BCP334
  name: newLogAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
  }
}

var applicationInsightsName = '${project}-${environment}-${applicationInsightsIdentifier}-${f.unique4()}'
resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: createNewLogAnalytics ? newLogAnalytics.id : existingLogAnalyticsId
  }
}
