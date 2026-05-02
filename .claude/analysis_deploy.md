# Kaesseli — Deploy & Infrastruktur

## Dockerfile (Repo-Root)
Multi-stage:
1. `mcr.microsoft.com/dotnet/aspnet:10.0 AS base` — Runtime, Port `8080`, `ASPNETCORE_URLS=http://+:8080`.
2. `mcr.microsoft.com/dotnet/sdk:10.0 AS build` — kopiert `Directory.Packages.props` + die drei `.csproj` (Backend, Contracts, Blazor) zuerst (Layer-Caching), `dotnet restore` für Backend und Blazor separat, dann Sources und **zwei separate Publish-Schritte**:
   - `dotnet publish Kaesseli/Kaesseli.csproj -c Release -o /app/publish /p:UseAppHost=false`
   - `dotnet publish Kaesseli.Client.Blazor/Kaesseli.Client.Blazor.csproj -c Release -o /app/blazor`
3. `final`: kopiert `/app/publish` ins WORKDIR und `/app/blazor/wwwroot` nach `wwwroot/`. `ENTRYPOINT ["dotnet", "Kaesseli.dll"]`.

Backend hostet die WASM-Dateien aus `wwwroot/` (`UseBlazorFrameworkFiles` + `MapFallbackToFile`). Der Vue-Client (`Kaesseli.Client/`) wird nicht ins Image kopiert.

## docker-compose (`Kaesseli.Deploy/docker-compose.yml`)
Nur das **Aspire Dashboard** für lokale Telemetry:
- Image `mcr.microsoft.com/dotnet/aspire-dashboard:9.2`
- UI: `http://localhost:18888`
- OTLP gRPC: `localhost:4317` (Container-Port 18889)
- OTLP HTTP: `localhost:4318` (Container-Port 18890)
- API-Key: `kaesseli-dev`
- Auth: Frontend `Unsecured`, OTLP `ApiKey`

`start-aspire.cmd` (Repo-Root) startet den Compose-Stack.

> Backend-Telemetry ist im Code **opt-in**: nur wenn `Otlp:Endpoint` gesetzt (Dev: `appsettings.Development.json` → `http://localhost:4317`). Prod-Default ist kein Endpoint → kein OTLP-Connect-Versuch.

## Deploy-Worker (`Kaesseli.Deploy/`)
Konsolen-App (kein Webhost), die:
1. Konfiguration aus `appsettings.json` + `appsettings.{env}.json` + `appsettings.user.json` baut.
2. `AddInfrastructureServices` aufruft (gleicher DI-Setup wie Backend).
3. `KaesseliContext.Database.EnsureCreatedAsync()` ausführt — legt Cosmos-Container an.
4. Wartet 5s, damit Cosmos-Collections verfügbar sind.
5. Wenn `DOTNET_ENVIRONMENT == "Development"`: ruft `SeedDevelopmentDataAsync` auf — legt Geschäftsjahr aktuelles Jahr, 8 Demokonten (Bank, Bargeld, Kreditkarte, Lohn, Lebensmittel, Miete, ÖV/Transport, Freizeit), Budget-Einträge, eine Bank-`TransactionSummary` mit zugewiesenen + offenen Demo-Transaktionen sowie zehn Journal-Einträge an. Skipt, falls bereits Konten existieren.

> Der Deploy-Worker initialisiert nur die DB; das Backend selbst macht `EnsureCreatedAsync` zusätzlich beim Startup im Background-Task (idempotent).

## Auth-relevante Konfiguration für Deploy
- **Backend** braucht: `Auth:Google:ClientId`, `Auth:Google:ClientSecret`, `Auth:Google:AllowedEmails`. ClientSecret und CosmosDb:Key gehören in KeyVault (`KeyVault:VaultUri` setzen) oder lokal in `appsettings.user.json`.
- **Client-Bundle** (`Kaesseli.Client.Blazor/wwwroot/appsettings.json`) braucht nur `Auth:Google:ClientId` und Redirect-URIs — **kein Secret**, weil das Bundle public ist und der Token-Tausch über den Backend-Proxy `/auth/google/token` läuft.
- Google OAuth Client (Web Application) muss `<origin>/authentication/login-callback` als Redirect-URI freigegeben haben.

## Branching / Aktueller Stand
- `main` ist der Default-Branch.
- Aktiver Working-Tree (Snapshot 2026-05-02): Branch `feature/dev-environment-banner`.
- Wichtige jüngere Commits:
  - `81c983a Visually mark the dev environment in the AppBar` — gelbes DEV-Badge wenn Host `-dev` enthält
  - `47374ad Prevent account deletion when journal entries exist` — `AccountInUseException` → 409
  - `eec00d6 Make app installable as a PWA` — manifest, icons, service-worker
  - `4720efc Require typing the entity name to confirm deletion` — `DeleteConfirmationDialog`
  - `937a7e5 Replace Basic Auth with Google Sign-In via OIDC` — kompletter Auth-Umbau
  - `3dd72ef Add YAML export/import for the chart of accounts` — `/account/plan` GET/POST

## Geplante / Vorhandene Infrastruktur
- Azure Cosmos DB Account `kaesseli-db.documents.azure.com` (Region "Switzerland North").
- Azure KeyVault optional über `KeyVault:VaultUri` + `DefaultAzureCredential` (lokal mit Azure CLI / Visual Studio Identity).
- `infra/`-Ordner enthält Infrastruktur-Skripte (nicht analysiert — bei Bedarf prüfen).

## Lokal starten
1. `start-aspire.cmd` → Aspire-Dashboard läuft auf `http://localhost:18888`.
2. Cosmos-DB-Emulator oder echter Endpoint in `appsettings.user.json` konfigurieren (`CosmosDb:Endpoint`, `CosmosDb:Key`, `CosmosDb:Database`).
3. **Google OAuth Web Client** anlegen, `Auth:Google:ClientId` + `Auth:Google:ClientSecret` in `Kaesseli/appsettings.user.json` setzen, `Auth:Google:AllowedEmails` mit der eigenen E-Mail füllen. Im Client-Bundle `Kaesseli.Client.Blazor/wwwroot/appsettings.json` nur die `ClientId` (kein Secret).
4. `Kaesseli.Deploy` einmal mit `DOTNET_ENVIRONMENT=Development` laufen lassen, um Container + Demo-Daten zu erzeugen.
5. `Kaesseli`-Projekt starten — hostet API + Blazor-Client unter `https://localhost:7123/` (Login geht über Google).

## CI / GitHub Actions
- `.github/`-Workflows existieren — bei Bedarf einsehen (`.github/workflows/`).
