@description('Location for all resources.')
param location string = resourceGroup().location

@description('Storage account name, globally unique. Rules and restrictions: https://docs.microsoft.com/en-us/azure/azure-resource-manager/management/resource-name-rules#microsoftstorage.')
@minLength(3)
@maxLength(24)
param storageAccoutName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: storageAccoutName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'BlobStorage'
  properties: {
    accessTier: 'Cool'
    supportsHttpsTrafficOnly: true
    encryption: {
      services: {
        blob: {
          enabled: true
        }
      }
      keySource: 'Microsoft.Storage'
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}
