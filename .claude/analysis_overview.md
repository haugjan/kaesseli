# Kaesseli – Solution Overview

> Stand: 2026-05-01. Stichprobenartiger Snapshot — Code immer gegen `main`/aktuellen Branch verifizieren.

## Zweck
Persönliches Doppelte-Buchhaltungs-Tool (Schweiz, Deutsch). Konten, Buchungsperioden, Journaleinträge, Budgets, Bankdatei-Import (PostFinance CSV / CAMT.053), automatische Zuordnung von Transaktionen zu Konten.

## Solution-Layout (`Kaesseli.slnx`)
```
Kaesseli/                 ASP.NET Core Web-API + Blazor-Host (net10.0)
Kaesseli.Client.Blazor/   Blazor WebAssembly Client (net10.0, MudBlazor)
Kaesseli.Client/          Legacy Vue/Quasar-Client (wird abgelöst — siehe blazor_migration.md)
Kaesseli.Contracts/       DTOs / Records, gemeinsam für Server & Client
Kaesseli.Deploy/          Konsolen-Worker für Cosmos-Init + Dev-Seeding
Tests/Kaesseli.Test/      xUnit-Tests (NSubstitute, Shouldly, Bogus, EF InMemory, TestHost)
Dockerfile                Build des Backend-Image (publisht WASM mit)
Directory.Packages.props  Zentrale Paketversionen (Central Package Management)
infra/                    Infrastruktur-Skripte
.claude/                  Claude-Notizen (diese Dateien, blazor_migration.md, settings)
```

## Tech-Stack
- **Runtime**: .NET 10 überall
- **Persistenz**: Azure Cosmos DB via EF Core (`Microsoft.EntityFrameworkCore.Cosmos`)
- **Auth**: HTTP Basic via Custom `BasicAuthHandler` (Username/Password aus Config). Es gibt zwar MSAL/Identity-Pakete in `Directory.Packages.props`, aber der Branch `feature/replace-b2c-with-basic-auth` zeigt: B2C wurde durch Basic Auth ersetzt.
- **API-Doku**: OpenAPI + Scalar (`/scalar`)
- **Telemetrie**: OpenTelemetry → OTLP-Endpoint (Default `localhost:4317` → Aspire Dashboard im Container)
- **UI**: Blazor WASM + MudBlazor 9.x; Sprache der UI ist Deutsch
- **Tests**: xUnit + NSubstitute + Shouldly + Bogus (`SmartFaker`) + EF InMemory + `Microsoft.AspNetCore.TestHost`

## Projekt-Referenzen (Compile-Time)
```
Kaesseli  ──► Kaesseli.Client.Blazor   (für UseBlazorFrameworkFiles)
Kaesseli  ──► Kaesseli.Contracts
Kaesseli.Client.Blazor ──► Kaesseli.Contracts
Kaesseli.Test ──► Kaesseli, Kaesseli.Contracts
Kaesseli.Deploy ──► (siehe Program.cs — nutzt Kaesseli intern)
```
`Kaesseli` macht InternalsVisibleTo `Kaesseli.Test`.

## Hosting-Modell
Backend hostet die Blazor-WASM-Files mit (`UseBlazorFrameworkFiles` + `MapFallbackToFile("/index.html")`). Der Branch `feature/replace-b2c-with-basic-auth` hat dies eingeführt, damit Basic Auth alles abdeckt. CORS-Origins für lokal: 7033/9000/9500/9501.

## Kern-Detaildateien
- `analysis_backend.md` — Projektaufbau Kaesseli (Endpoints, Handlers, EF, Auth)
- `analysis_frontend.md` — Blazor-Client (Pages, Services, State)
- `analysis_testing.md` — Test-Patterns
- `analysis_deploy.md` — Dockerfile, Compose, Deploy-Worker, Aspire-Dashboard
- `blazor_migration.md` — Migrationsplan Vue → Blazor (separat gepflegt)
