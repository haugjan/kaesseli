$ErrorActionPreference = "Stop"

# ─── Configuration ──────────────────────────────────────────────
$ResourceGroup     = "RG_Kaesseli"
$Location          = "switzerlandnorth"
$RegistryName      = "kaesselicr"
$KeyVaultName      = "kaesseli-kv"
$LogAnalyticsName  = "kaesseli-logs"
$ContainerEnvName  = "kaesseli-env"
$GitHubRepo        = "haugjan/kaesseli"

# ─── Resource Group ─────────────────────────────────────────────
Write-Host "==> Creating resource group..."
az group create --name $ResourceGroup --location $Location -o none

# ─── Azure Container Registry ──────────────────────────────────
Write-Host "==> Creating container registry..."
az acr create `
  --resource-group $ResourceGroup `
  --name $RegistryName `
  --sku Basic `
  --admin-enabled true `
  -o none

$AcrLoginServer = az acr show --name $RegistryName --query loginServer -o tsv
$AcrPassword = az acr credential show --name $RegistryName --query "passwords[0].value" -o tsv

# ─── Key Vault ──────────────────────────────────────────────────
Write-Host "==> Creating key vault..."
az keyvault create `
  --resource-group $ResourceGroup `
  --name $KeyVaultName `
  --location $Location `
  --enable-rbac-authorization true `
  -o none

$KeyVaultId = az keyvault show --name $KeyVaultName --query id -o tsv
$CurrentUserId = az ad signed-in-user show --query id -o tsv

az role assignment create `
  --role "Key Vault Secrets Officer" `
  --assignee $CurrentUserId `
  --scope $KeyVaultId `
  -o none

Write-Host "==> Seeding key vault secrets (only if they don't exist yet)..."
$SecretsToSeed = @{
    "CosmosDb--Key"       = "REPLACE_WITH_COSMOS_KEY"
    "AzureAdB2C--TenantId" = "d8448c66-e344-4009-8169-8083b06994b4"
    "AzureAdB2C--ClientId" = "c5190367-84f5-4ea7-8fc4-086d0c1abe24"
}

foreach ($SecretName in $SecretsToSeed.Keys) {
    $Existing = az keyvault secret show --vault-name $KeyVaultName --name $SecretName --query value -o tsv 2>$null
    if ($Existing) {
        Write-Host "    Secret '$SecretName' already exists, skipping"
    } else {
        Write-Host "    Setting secret '$SecretName'..."
        az keyvault secret set --vault-name $KeyVaultName --name $SecretName --value $SecretsToSeed[$SecretName] -o none
    }
}

# ─── Log Analytics Workspace ───────────────────────────────────
Write-Host "==> Creating log analytics workspace..."
az monitor log-analytics workspace create `
  --resource-group $ResourceGroup `
  --workspace-name $LogAnalyticsName `
  --location $Location `
  -o none

$LogAnalyticsCustomerId = az monitor log-analytics workspace show `
  --resource-group $ResourceGroup `
  --workspace-name $LogAnalyticsName `
  --query customerId -o tsv
$LogAnalyticsKey = az monitor log-analytics workspace get-shared-keys `
  --resource-group $ResourceGroup `
  --workspace-name $LogAnalyticsName `
  --query primarySharedKey -o tsv

# ─── Container Apps Environment ────────────────────────────────
Write-Host "==> Creating container apps environment..."
az containerapp env create `
  --resource-group $ResourceGroup `
  --name $ContainerEnvName `
  --location $Location `
  --logs-workspace-id $LogAnalyticsCustomerId `
  --logs-workspace-key $LogAnalyticsKey `
  -o none

# ─── Container Apps (Dev & Prod) ───────────────────────────────
foreach ($EnvName in @("dev", "prod")) {
    $AppName = "kaesseli-$EnvName"
    Write-Host "==> Creating container app: $AppName..."

    az containerapp create `
      --resource-group $ResourceGroup `
      --name $AppName `
      --environment $ContainerEnvName `
      --image "mcr.microsoft.com/dotnet/samples:aspnetapp" `
      --target-port 8080 `
      --ingress external `
      --min-replicas 0 `
      --max-replicas 2 `
      --cpu 0.25 `
      --memory 0.5Gi `
      --registry-server $AcrLoginServer `
      --registry-username $RegistryName `
      --registry-password $AcrPassword `
      --env-vars `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "KeyVault__VaultUri=https://${KeyVaultName}.vault.azure.net/" `
      -o none

    # Enable system-assigned managed identity
    $PrincipalId = az containerapp identity assign `
      --resource-group $ResourceGroup `
      --name $AppName `
      --system-assigned `
      --query principalId -o tsv

    # Grant Key Vault Secrets User role
    az role assignment create `
      --role "Key Vault Secrets User" `
      --assignee $PrincipalId `
      --scope $KeyVaultId `
      -o none

    Write-Host "    $AppName identity: $PrincipalId -> Key Vault access granted"
}

# ─── GitHub Actions: Service Principal with Federated Credentials ─
Write-Host "==> Setting up service principal for GitHub Actions..."
$SpAppId = az ad app list --display-name "kaesseli-github-deploy" --query "[0].appId" -o tsv
if (-not $SpAppId) {
    Write-Host "    Creating app registration..."
    $SpAppId = az ad app create --display-name "kaesseli-github-deploy" --query appId -o tsv
} else {
    Write-Host "    App registration already exists ($SpAppId)"
}

$SpObjectId = az ad sp show --id $SpAppId --query id -o tsv 2>$null
if (-not $SpObjectId) {
    Write-Host "    Creating service principal..."
    $SpObjectId = az ad sp create --id $SpAppId --query id -o tsv
} else {
    Write-Host "    Service principal already exists ($SpObjectId)"
}

$SubscriptionId = az account show --query id -o tsv
$TenantId = az account show --query tenantId -o tsv

az role assignment create `
  --role "Contributor" `
  --assignee $SpObjectId `
  --scope "/subscriptions/${SubscriptionId}/resourceGroups/${ResourceGroup}" `
  -o none 2>$null

$AcrId = az acr show --name $RegistryName --query id -o tsv
az role assignment create `
  --role "AcrPush" `
  --assignee $SpObjectId `
  --scope $AcrId `
  -o none 2>$null

# Federated credentials for GitHub Actions
$SpAppObjectId = az ad app show --id $SpAppId --query id -o tsv

$Subjects = @(
    "repo:${GitHubRepo}:ref:refs/heads/main",
    "repo:${GitHubRepo}:environment:dev",
    "repo:${GitHubRepo}:environment:prod"
)

$ExistingCreds = az ad app federated-credential list --id $SpAppObjectId --query "[].subject" -o tsv 2>$null

foreach ($Subject in $Subjects) {
    if ($ExistingCreds -contains $Subject) {
        Write-Host "    Federated credential already exists: $Subject"
        continue
    }

    $FedName = ($Subject -replace '[^a-zA-Z0-9]', '-')
    if ($FedName.Length -gt 64) { $FedName = $FedName.Substring($FedName.Length - 64) }

    $ParamsObj = @{
        name      = $FedName
        issuer    = "https://token.actions.githubusercontent.com"
        subject   = $Subject
        audiences = @("api://AzureADTokenExchange")
    }
    $TempFile = [System.IO.Path]::GetTempFileName()
    $ParamsObj | ConvertTo-Json | Set-Content -Path $TempFile -Encoding utf8

    Write-Host "    Creating federated credential: $Subject"
    az ad app federated-credential create --id $SpAppObjectId --parameters "@$TempFile" -o none
    Remove-Item $TempFile
}

# ─── Summary ───────────────────────────────────────────────────
$DevFqdn = az containerapp show --resource-group $ResourceGroup --name kaesseli-dev --query "properties.configuration.ingress.fqdn" -o tsv
$ProdFqdn = az containerapp show --resource-group $ResourceGroup --name kaesseli-prod --query "properties.configuration.ingress.fqdn" -o tsv

Write-Host ""
Write-Host "============================================================"
Write-Host " Setup complete!"
Write-Host "============================================================"
Write-Host ""
Write-Host " Container Registry: $AcrLoginServer"
Write-Host " Key Vault:          https://${KeyVaultName}.vault.azure.net/"
Write-Host " Dev URL:            https://${DevFqdn}"
Write-Host " Prod URL:           https://${ProdFqdn}"
Write-Host ""
Write-Host " GitHub Secrets (add to repo settings):"
Write-Host "   AZURE_CLIENT_ID:       $SpAppId"
Write-Host "   AZURE_TENANT_ID:       $TenantId"
Write-Host "   AZURE_SUBSCRIPTION_ID: $SubscriptionId"
Write-Host "   ACR_LOGIN_SERVER:      $AcrLoginServer"
Write-Host ""
Write-Host " Key Vault secrets to update:"
Write-Host "   CosmosDb--Key:  your actual Cosmos DB key"
Write-Host "============================================================"
