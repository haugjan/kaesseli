# Kaesseli — Claude-Einstieg

Persönliches Doppelte-Buchhaltungs-Tool (Schweiz, Deutsch). ASP.NET Core Web-API + Blazor WASM, .NET 10, Cosmos DB, **Google Sign-In via OIDC** (JWT-Bearer mit Email-Allowlist). Als PWA installierbar.

> **Wichtig:** Detail-Notizen liegen unter `.claude/analysis_*.md` (Stand 2026-05-02). Bei Konflikt mit dem aktuellen Code immer den Code als Quelle der Wahrheit nehmen.

## Erst lesen — passend zur Aufgabe

| Aufgabe | Datei |
|---|---|
| Solution insgesamt verstehen, Projekt-Layout, Tech-Stack | [.claude/analysis_overview.md](./.claude/analysis_overview.md) |
| Backend ändern (Endpoints, Handler, EF, Auth, Konfiguration) | [.claude/analysis_backend.md](./.claude/analysis_backend.md) |
| Blazor-Client ändern (Pages, Services, State, Layout) | [.claude/analysis_frontend.md](./.claude/analysis_frontend.md) |
| Tests schreiben/anpassen (xUnit, NSubstitute, SmartFaker, TestHost) | [.claude/analysis_testing.md](./.claude/analysis_testing.md) |
| Docker, Compose, Aspire-Dashboard, Deploy-Worker, Lokal-Setup | [.claude/analysis_deploy.md](./.claude/analysis_deploy.md) |
| Vue → Blazor Migrationsplan (Legacy-Kontext) | [.claude/blazor_migration.md](./.claude/blazor_migration.md) |

## Solution auf einen Blick

```
Kaesseli/                 ASP.NET Core Web-API + Blazor-Host (net10.0)
Kaesseli.Client.Blazor/   Blazor WebAssembly Client (MudBlazor)
Kaesseli.Client/          Vue/Quasar Legacy-Client — wird abgelöst
Kaesseli.Contracts/       Geteilte DTOs/Records
Kaesseli.Deploy/          Konsolen-Worker für Cosmos-Init + Dev-Seed
Tests/Kaesseli.Test/      xUnit-Tests
infra/                    Infrastruktur-Skripte
.claude/                  Notizen + Analyse-Snapshots
```

Backend hostet die Blazor-WASM-Files mit (`UseBlazorFrameworkFiles` + `MapFallbackToFile`). Auth ist **Google OIDC** mit Email-Allowlist (`Auth:Google:AllowedEmails`); ClientSecret bleibt serverseitig hinter dem Token-Proxy `/auth/google/token`. Default-Policy schützt jede Route — explizit anonym nur `/healthz`, OIDC-Proxies, OpenAPI/Scalar (Dev), SPA-Fallback. Cosmos legt Container per `EnsureCreatedAsync` an — keine EF-Migrations.

## Konventionen kurz

- **Vertical Slices** im Backend: jeder Use-Case ist `static class XAction { record Query; interface IHandler; class Handler : IHandler }`. Endpoints rufen nur `IHandler` aus DI.
- **Domain-Klassen** mit `private` Ctor + statischer `Create(...)`-Factory; Validation in der Factory.
- **Primary Constructors** überall.
- **UI-Sprache: Deutsch.** Code/Bezeichner auf Englisch.
- **Tests:** Handler direkt instanziieren mit `Substitute.For<IDep>()`; API-Tests mit `WebApplication.CreateBuilder().WebHost.UseTestServer()`.
- **Blazor-Client ist die Wahrheit.** Vue-Client (`Kaesseli.Client/`) wird nicht mehr ausgebaut.

## Lokal starten (Kurzfassung)

1. `start-aspire.cmd` → Aspire-Dashboard auf `http://localhost:18888`.
2. Backend `appsettings.user.json`: `CosmosDb:*` + `Auth:Google:ClientId|ClientSecret|AllowedEmails`. Client-Bundle `Kaesseli.Client.Blazor/wwwroot/appsettings.json`: nur `Auth:Google:ClientId` (kein Secret im Public-Bundle).
3. `Kaesseli.Deploy` einmal mit `DOTNET_ENVIRONMENT=Development` laufen lassen → Container + Demo-Daten.
4. `Kaesseli` starten → API + Blazor unter `https://localhost:7123/` (Login via Google).

Details und CORS-Origins in [analysis_deploy.md](./.claude/analysis_deploy.md).
