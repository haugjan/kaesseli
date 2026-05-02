# Kaesseli – Solution Overview

> Stand: 2026-05-02. Stichprobenartiger Snapshot — Code immer gegen `main`/aktuellen Branch verifizieren.

## Zweck
Persönliches Doppelte-Buchhaltungs-Tool (Schweiz, Deutsch). Konten, Buchungsperioden, Journaleinträge, Budgets, Bankdatei-Import (PostFinance CSV / CAMT.053), automatische Zuordnung von Transaktionen zu Konten, YAML-Export/Import des Kontoplans. Als PWA installierbar.

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
- **Auth**: **Google Sign-In via OIDC** — Backend validiert Google-ID-Tokens als JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), zusätzlich Email-Allowlist (`Auth:Google:AllowedEmails`). Client nutzt `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (`AddOidcAuthentication`). ClientSecret bleibt serverseitig — Backend stellt einen Token-Proxy bereit (`/auth/google/token`), damit der Public Client kein Secret braucht. Basic Auth und Azure B2C/MSAL sind raus (Commit `937a7e5 Replace Basic Auth with Google Sign-In via OIDC`).
- **API-Doku**: OpenAPI + Scalar (Dev-only, `/scalar`, `AllowAnonymous`)
- **Telemetrie**: OpenTelemetry → OTLP-gRPC; **nur aktiv, wenn `Otlp:Endpoint` konfiguriert** (Dev: Aspire-Dashboard auf `localhost:4317`; Prod: kein Auto-Connect)
- **UI**: Blazor WASM + MudBlazor 9.x; Sprache der UI ist Deutsch; PWA (manifest.webmanifest, service-worker.js)
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
Backend hostet die Blazor-WASM-Files mit (`UseBlazorFrameworkFiles` + `MapFallbackToFile("/index.html")`). Im Dockerfile werden Backend und Blazor getrennt gepublished und ins selbe Image kopiert. CORS-Origins für lokal: 7033/9000/9500/9501.

## Authorization-Modell
- Alle Endpoints sind per Default geschützt: `SetFallbackPolicy` verlangt `RequireAuthenticatedUser` + `EmailAllowlistRequirement`.
- Email-Allowlist liegt in `Auth:Google:AllowedEmails` (Array). Token-Claim `email_verified` muss `"true"` sein.
- Explizit anonym: `MapHealthChecks("/healthz")`, OpenAPI/Scalar (Dev), Token-/Metadata-Proxy unter `/auth/google/...`, SPA-Fallback (`MapFallbackToFile`).

## Kern-Detaildateien
- `analysis_backend.md` — Projektaufbau Kaesseli (Endpoints, Handlers, EF, Auth)
- `analysis_frontend.md` — Blazor-Client (Pages, Services, State)
- `analysis_testing.md` — Test-Patterns
- `analysis_deploy.md` — Dockerfile, Compose, Deploy-Worker, Aspire-Dashboard
- `blazor_migration.md` — Migrationsplan Vue → Blazor (im Wesentlichen abgeschlossen)
