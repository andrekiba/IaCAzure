import * as f from './../functions.bicep'

@description('The existing Sql Server Name')
param existingSqlServeName string = ''

param allowedIpAddresses string
param allowAzure bool

resource sqlSrv 'Microsoft.Sql/servers@2024-05-01-preview' existing = {
  name: existingSqlServeName
}

var hasAllowedIpAddresses = !empty(allowedIpAddresses)
var ipAddressList = split(allowedIpAddresses, ',')

resource fwRule 'Microsoft.Sql/servers/firewallRules@2014-04-01' = [for ip in ipAddressList: if (hasAllowedIpAddresses) {
  name: 'allow_${ip}'
  parent: sqlSrv
  properties: {
    startIpAddress: ip
    endIpAddress: ip
  }
}]

resource fwRuleAzure 'Microsoft.Sql/servers/firewallrules@2014-04-01' = if (allowAzure) {
  parent: sqlSrv
  name: 'allow_Azure'
  properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '255.255.255.255'
  }
}
