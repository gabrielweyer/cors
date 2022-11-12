@description('Static Web App location.')
@allowed(['westus2','centralus','eastus2','westeurope','eastasia'])
param staticWebAppLocation string

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: 'corsst'
  location: staticWebAppLocation
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
  }
}

resource hostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: 'cors-plan'
  location: staticWebAppLocation
  kind: ''
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
    size: 'Y1'
    family: 'Y'
    capacity: 0
  }
  properties: {}
}

var functionAppName = 'cors-func'
resource apiApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: staticWebAppLocation
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    httpsOnly: true
    serverFarmId: hostingPlan.id
  }
}

var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${listKeys(storageAccount.id, storageAccount.apiVersion).keys[0].value};EndpointSuffix=${environment().suffixes.storage}'
resource apiAppSettings 'Microsoft.Web/sites/config@2021-03-01' = {
  parent: apiApp
  name: 'appsettings'
  properties: {
    AzureWebJobsStorage: storageConnectionString
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageConnectionString
    WEBSITE_CONTENTSHARE: functionAppName
    WEBSITE_RUN_FROM_PACKAGE: '1'
  }
}

resource staticWebApp 'Microsoft.Web/staticSites@2022-03-01' = {
  name: 'cors-stapp'
  location: staticWebAppLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    branch: 'main'
    provider: 'GitHub'
    repositoryUrl: 'https://github.com/gabrielweyer/jwt-viewer'
    buildProperties: {
      skipGithubActionWorkflowGeneration: true
    }
  }
}
