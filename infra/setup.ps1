$ErrorActionPreference = "Stop"

# ─── Configuration ──────────────────────────────────────────────
$ResourceGroup     = "RG_Kaesseli"
$Location          = "switzerlandnorth"
$RegistryName      = "kaesselicr"
$KeyVaultName      = "kaesseli-kv"
$LogAnalyticsName  = "kaesseli-logs"
$ContainerEnvName  = "kaesseli-env"
$GitHubRepo        = "haugjan/kaesseli"
$CosmosAccountName = "kaesseli-db"

# ─── Step definitions ──────────────────────────────────────────
$Steps = @(
    "Resource Group",
    "Container Registry",
    "Cosmos DB (Serverless)",
    "Key Vault & Secrets",
    "Log Analytics Workspace",
    "Container Apps Environment",
    "Container Apps (Dev & Prod)",
    "GitHub Actions Service Principal & Federated Credentials"
)

# ─── Step selection ────────────────────────────────────────────
Write-Host ""
Write-Host "Available steps:"
for ($i = 0; $i -lt $Steps.Count; $i++) {
    Write-Host "  [$($i + 1)] $($Steps[$i])"
}
Write-Host ""
$Selection = Read-Host "Enter step numbers (comma-separated) or 'all' to run everything"

if ($Selection -eq "all") {
    $SelectedSteps = 1..$Steps.Count
} else {
    $SelectedSteps = $Selection -split ',' | ForEach-Object { [int]$_.Trim() }
    $Invalid = $SelectedSteps | Where-Object { $_ -lt 1 -or $_ -gt $Steps.Count }
    if ($Invalid) {
        Write-Error "Invalid step(s): $($Invalid -join ', '). Valid: 1-$($Steps.Count)"
        return
    }
}

Write-Host ""
Write-Host "Running steps: $(($SelectedSteps | ForEach-Object { "$_ ($($Steps[$_ - 1]))" }) -join ', ')"
Write-Host ""

# ─── Helper: check if step is selected ─────────────────────────
function Should-Run($StepNumber) { $StepNumber -in $SelectedSteps }

# ─── Step 1: Resource Group ────────────────────────────────────
if (Should-Run 1) {
    Write-Host "==> [1/8] Creating resource group..."
    az group create --name $ResourceGroup --location $Location -o none
}

# ─── Step 2: Azure Container Registry ─────────────────────────
if (Should-Run 2) {
    Write-Host "==> [2/8] Creating container registry..."
    az acr create `
      --resource-group $ResourceGroup `
      --name $RegistryName `
      --sku Basic `
      --admin-enabled true `
      -o none
}

# ─── Step 3: Cosmos DB (Serverless) ──────────────────────────
if (Should-Run 3) {
    Write-Host "==> [3/8] Creating Cosmos DB account (serverless)..."
    $ExistingCosmos = az cosmosdb show --name $CosmosAccountName --resource-group $ResourceGroup --query name -o tsv 2>$null
    if ($ExistingCosmos) {
        Write-Host "    Cosmos DB account '$CosmosAccountName' already exists, skipping account creation"
    } else {
        az cosmosdb create `
          --name $CosmosAccountName `
          --resource-group $ResourceGroup `
          --locations regionName=$Location `
          --capabilities EnableServerless `
          --default-consistency-level Session `
          -o none
    }

    Write-Host "    Creating databases (if they don't exist)..."
    foreach ($DbName in @("kaesseli", "dev-kaesseli")) {
        $ExistingDb = az cosmosdb sql database show `
          --account-name $CosmosAccountName `
          --resource-group $ResourceGroup `
          --name $DbName `
          --query name -o tsv 2>$null
        if ($ExistingDb) {
            Write-Host "    Database '$DbName' already exists, skipping"
        } else {
            Write-Host "    Creating database '$DbName'..."
            az cosmosdb sql database create `
              --account-name $CosmosAccountName `
              --resource-group $ResourceGroup `
              --name $DbName `
              -o none
        }
    }
}

# Fetch Cosmos DB key (needed by step 4)
if (Should-Run 4) {
    $CosmosKey = az cosmosdb keys list `
      --name $CosmosAccountName `
      --resource-group $ResourceGroup `
      --query primaryMasterKey -o tsv 2>$null
}

# Fetch ACR details (needed by steps 7 and 8)
if (Should-Run 7 -or Should-Run 8) {
    $AcrLoginServer = az acr show --name $RegistryName --query loginServer -o tsv
    $AcrPassword = az acr credential show --name $RegistryName --query "passwords[0].value" -o tsv
}

# ─── Step 4: Key Vault & Secrets ──────────────────────────────
if (Should-Run 4) {
    Write-Host "==> [4/8] Creating key vault..."
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

    Write-Host "    Seeding key vault secrets (only if they don't exist yet)..."
    $SecretsToSeed = @{
        "CosmosDb--Key"          = if ($CosmosKey) { $CosmosKey } else { "REPLACE_WITH_COSMOS_KEY" }
        "Auth--Google--ClientId" = "REPLACE_WITH_GOOGLE_OAUTH_CLIENT_ID"
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
}

# Fetch Key Vault ID (needed by step 7)
if (Should-Run 7) {
    if (-not $KeyVaultId) {
        $KeyVaultId = az keyvault show --name $KeyVaultName --query id -o tsv
    }
}

# ─── Step 5: Log Analytics Workspace ──────────────────────────
if (Should-Run 5) {
    Write-Host "==> [5/8] Creating log analytics workspace..."
    az monitor log-analytics workspace create `
      --resource-group $ResourceGroup `
      --workspace-name $LogAnalyticsName `
      --location $Location `
      -o none
}

# ─── Step 6: Container Apps Environment ───────────────────────
if (Should-Run 6) {
    Write-Host "==> [6/8] Creating container apps environment..."
    $LogAnalyticsCustomerId = az monitor log-analytics workspace show `
      --resource-group $ResourceGroup `
      --workspace-name $LogAnalyticsName `
      --query customerId -o tsv
    $LogAnalyticsKey = az monitor log-analytics workspace get-shared-keys `
      --resource-group $ResourceGroup `
      --workspace-name $LogAnalyticsName `
      --query primarySharedKey -o tsv

    az containerapp env create `
      --resource-group $ResourceGroup `
      --name $ContainerEnvName `
      --location $Location `
      --logs-workspace-id $LogAnalyticsCustomerId `
      --logs-workspace-key $LogAnalyticsKey `
      -o none
}

# ─── Step 7: Container Apps (Dev & Prod) ──────────────────────
if (Should-Run 7) {
    foreach ($EnvName in @("dev", "prod")) {
        $AppName = "kaesseli-$EnvName"
        Write-Host "==> [7/8] Creating container app: $AppName..."

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
}

# ─── Step 8: GitHub Actions Service Principal & Federated Credentials ─
if (Should-Run 8) {
    Write-Host "==> [8/8] Setting up service principal for GitHub Actions..."
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

    if (-not $AcrLoginServer) {
        $AcrLoginServer = az acr show --name $RegistryName --query loginServer -o tsv
    }
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
}

# ─── Summary ───────────────────────────────────────────────────
if (-not $SpAppId)       { $SpAppId = az ad app list --display-name "kaesseli-github-deploy" --query "[0].appId" -o tsv 2>$null }
if (-not $AcrLoginServer) { $AcrLoginServer = az acr show --name $RegistryName --query loginServer -o tsv 2>$null }
if (-not $SubscriptionId) { $SubscriptionId = az account show --query id -o tsv }
if (-not $TenantId)       { $TenantId = az account show --query tenantId -o tsv }
$DevFqdn = az containerapp show --resource-group $ResourceGroup --name kaesseli-dev --query "properties.configuration.ingress.fqdn" -o tsv 2>$null
$ProdFqdn = az containerapp show --resource-group $ResourceGroup --name kaesseli-prod --query "properties.configuration.ingress.fqdn" -o tsv 2>$null

Write-Host ""
Write-Host "============================================================"
Write-Host " Setup complete!"
Write-Host "============================================================"
Write-Host ""
Write-Host " Container Registry: $AcrLoginServer"
Write-Host " Cosmos DB:          https://${CosmosAccountName}.documents.azure.com:443/"
Write-Host " Key Vault:          https://${KeyVaultName}.vault.azure.net/"
Write-Host " Dev URL:            https://${DevFqdn}"
Write-Host " Prod URL:           https://${ProdFqdn}"
Write-Host ""
Write-Host " GitHub Secrets (add to repo settings):"
Write-Host "   AZURE_CLIENT_ID:       $SpAppId"
Write-Host "   AZURE_TENANT_ID:       $TenantId"
Write-Host "   AZURE_SUBSCRIPTION_ID: $SubscriptionId"
Write-Host "   ACR_LOGIN_SERVER:      $AcrLoginServer"
Write-Host "============================================================"
