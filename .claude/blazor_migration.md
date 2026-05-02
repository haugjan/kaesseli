---
name: Blazor Migration Analysis
description: Migration des Vue/Quasar-Clients (Kaesseli.Client) zu Blazor WebAssembly — im Wesentlichen abgeschlossen, Vue-Client eingefroren
type: project
---

## Status: weitgehend abgeschlossen

Der Blazor-Client (`Kaesseli.Client.Blazor/`) ist die produktive UI; der Vue-Client (`Kaesseli.Client/`) ist eingefroren und wird nicht mehr ins Docker-Image gepublished. Bei neuen Features ausschließlich Blazor anfassen. Diese Datei hält den ursprünglichen Migrationsplan als historischen Kontext fest.

**Tatsächliche Auth-Lösung (anders als ursprünglich geplant):** Statt Azure AD/MSAL ist der Endzustand **Google Sign-In via OIDC** mit Email-Allowlist auf dem Backend (siehe `analysis_backend.md::GoogleAuth.cs`). Davor gab es ein Zwischenstadium mit Basic Auth — beides ist Geschichte.

---

## Ziel (ursprünglich)
Den bestehenden Vue 3 + Quasar-Client durch eine Blazor WebAssembly App ersetzen.

**Why:** Einheitliche C#-Sprache über Frontend und Backend, geteilte Modelle, sauberere Auth-Integration, kein JS/TS-Toolchain-Overhead.

**How to apply:** Bei neuen Features Blazor verwenden; nicht mehr Vue. Shared-DTOs liegen in `Kaesseli.Contracts`.

---

## Vue-Client (Legacy)

- **Stack**: Vue 3, Quasar Framework, TypeScript, Axios
- **Kein State-Management-Framework** (kein Pinia/Vuex) – component-local mit `ref()`
- **Auth**: Azure MSAL konfiguriert aber deaktiviert (`authConfig.js`)
- **Routing**: Hash-based, 6 Routen
- **API-Base**: `https://localhost:7123/`
- **Sprache**: Deutsch (UI), TypeScript (Code)

### Vue-Komponenten (was migriert wurde)
| Vue | Blazor-Pendant | Route |
|---|---|---|
| KaesseliHome | `Pages/Home.razor` | `/` |
| KaesseliAccounts | `Pages/Accounts.razor` | `/accounts` |
| KaesseliAccountTable | `Pages/AccountTable.razor` | `/accountTable/{...}` |
| KaesseliTransactions | `Pages/Transactions.razor` | `/transactions` |
| KaesseliImport | `Pages/Import.razor` | `/import` |
| KaesseliAssign | `Pages/Assign.razor` | `/assign` |

Zusätzlich neu im Blazor-Client (kein Vue-Pendant): `SettingsAccounts`, `AccountingPeriods`, `AccountingPeriodDetail`, `AdminCleanupOrphans`, `Authentication`.

---

## Tatsächliche Migrations-Mappings

| Vue/Quasar | Blazor (umgesetzt) |
|---|---|
| `ref()`, `onMounted()` | `@code {}`, `OnInitializedAsync()` |
| Vue Router | `@page "/route"`, `NavigationManager` |
| Axios | `HttpClient` via DI |
| Quasar (Material Design) | **MudBlazor 9.x** |
| LocalStorage direkt | `IJSRuntime.InvokeAsync("localStorage.*")` (kein Blazored.LocalStorage genutzt) |
| MSAL-JS | `Microsoft.AspNetCore.Components.WebAssembly.Authentication` (`AddOidcAuthentication`) gegen Google |
| State-Container | `Services/AccountingPeriodState.cs` (Singleton, `event Action OnChange`) |

### KaesseliAssign (komplexeste Komponente)
- Ctrl+Click für Split: `MouseEventArgs.CtrlKey`
- Live-Filter: `@oninput` + LINQ
- Debounced Automation-Counter: `Task.Delay` + `CancellationToken`
- Split-Betragsverteilung: C# `decimal` (keine JS-Floating-Point-Probleme)

### Auth (Endzustand)
Google OIDC im Client (`Program.cs`):
```csharp
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth:Google", options.ProviderOptions);
    options.ProviderOptions.Authority = "https://accounts.google.com";
    options.ProviderOptions.MetadataUrl = $"{baseAddress}/auth/google/.well-known/openid-configuration";
    // ResponseType "code", Scopes openid/email/profile, NameClaim "email"
});
```
Backend-Validierung über `JwtBearer` + Email-Allowlist; ClientSecret bleibt serverseitig, der Public Client tauscht den Code über den Backend-Token-Proxy.

---

## API-Endpunkte
Siehe `analysis_backend.md::Endpoint-Übersicht`. Backend wurde während der Migration um YAML-Kontoplan-Export/Import (`/account/plan`), Wartungs-Endpoint (`/admin/cleanupOrphanedAccountReferences`), Health-Check (`/healthz`) und die OIDC-Proxies (`/auth/google/...`) erweitert.
