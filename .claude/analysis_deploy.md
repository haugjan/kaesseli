# Kaesseli — Deploy & Infrastruktur

## Dockerfile (Repo-Root)
Multi-stage:
1. `mcr.microsoft.com/dotnet/sdk:10.0 AS build` — kopiert `Directory.Packages.props` + die drei `.csproj`-Dateien zuerst (Layer-Caching), `dotnet restore` für `Kaesseli.csproj`, dann Sources und `dotnet publish Kaesseli/Kaesseli.csproj -c Release -o /app/publish /p:UseAppHost=false`.
2. `mcr.microsoft.com/dotnet/aspnet:10.0 AS base` — runtime, exposed Port `8080`, `ASPNETCORE_URLS=http://+:8080`, `ENTRYPOINT ["dotnet", "Kaesseli.dll"]`.

Da `Kaesseli` per ProjectReference auf `Kaesseli.Client.Blazor` verweist, wird der Blazor-Client beim Publish mitgepacked und vom Backend ausgeliefert. Der Vue-Client (`Kaesseli.Client/`) wird nicht ins Image kopiert.

## docker-compose (`Kaesseli.Deploy/docker-compose.yml`)
Nur das **Aspire Dashboard** für lokale Telemetry:
- Image `mcr.microsoft.com/dotnet/aspire-dashboard:9.2`
- UI: `http://localhost:18888`
- OTLP gRPC: `localhost:4317` (Container-Port 18889)
- OTLP HTTP: `localhost:4318` (Container-Port 18890)
- API-Key: `kaesseli-dev`
- Auth: Frontend `Unsecured`, OTLP `ApiKey`

`start-aspire.cmd` (Repo-Root) startet vermutlich den Compose-Stack.

## Deploy-Worker (`Kaesseli.Deploy/`)
Konsolen-App (kein Webhost), die:
1. Konfiguration aus `appsettings.json` + `appsettings.{env}.json` + `appsettings.user.json` baut.
2. `AddInfrastructureServices` aufruft (gleicher DI-Setup wie Backend).
3. `KaesseliContext.Database.EnsureCreatedAsync()` ausführt — legt Cosmos-Container an.
4. Wartet 5s, damit Cosmos-Collections verfügbar sind.
5. Wenn `DOTNET_ENVIRONMENT == "Development"`: ruft `SeedDevelopmentDataAsync` auf — legt Geschäftsjahr aktuelles Jahr, 8 Demokonten (Bank, Bargeld, Kreditkarte, Lohn, Lebensmittel, Miete, Transport, Freizeit), Budget-Einträge, eine Bank-`TransactionSummary` mit zugewiesenen + offenen Demo-Transaktionen sowie zehn Journal-Einträge an. Skipt, falls bereits Konten existieren.

## Branching / Aktueller Branch
- `main` ist der Default-Branch.
- Aktiver Feature-Branch: `feature/replace-b2c-with-basic-auth`. Letzte Commits zeigen:
  - "Expand allowed Bash commands and remove MSAL script from index.html"
  - "Host Blazor WASM from backend so Basic Auth covers everything"
  - "Fix Blazor API calls with explicit Basic Auth header from config"
  - "Use browser-cached Basic Auth credentials for API calls"
  - "Send Basic Auth header from Blazor client on all API requests"
- Aktueller Working-Tree: `M Dockerfile`, `M Kaesseli.Client.Blazor/Program.cs`.

## Geplante / Vorhandene Infrastruktur
- Azure Cosmos DB Account `kaesseli-db.documents.azure.com` (Region "Switzerland North").
- Azure KeyVault optional über `KeyVault:VaultUri` + `DefaultAzureCredential` (lokal mit Azure CLI / Visual Studio Identity).
- `infra/`-Ordner enthält Infrastruktur-Skripte (nicht analysiert — bei Bedarf prüfen).

## Lokal starten
1. `start-aspire.cmd` → Aspire-Dashboard läuft auf `http://localhost:18888`.
2. Cosmos-DB-Emulator oder echter Endpoint in `appsettings.user.json` konfigurieren (`CosmosDb:Endpoint`, `CosmosDb:Key`, `CosmosDb:Database`).
3. `BasicAuth:Username` / `BasicAuth:Password` in `appsettings.user.json` (Backend) und `Kaesseli.Client.Blazor/wwwroot/appsettings.json` (Client) konsistent setzen.
4. `Kaesseli.Deploy` einmal mit `DOTNET_ENVIRONMENT=Development` laufen lassen, um Container + Demo-Daten zu erzeugen.
5. `Kaesseli`-Projekt starten — hostet API + Blazor-Client unter `https://localhost:7123/` (siehe `ApiBaseUrl` im Client).

## CI / GitHub Actions
- `.github/`-Workflows existieren — bei Bedarf einsehen (`.github/workflows/`).
