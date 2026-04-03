---
name: Blazor Migration Analysis
description: Geplante Migration des Vue/Quasar-Clients (Kaesseli.Client) zu Blazor WebAssembly (clientside)
type: project
---

## Ziel
Den bestehenden Vue 3 + Quasar-Client durch eine Blazor WebAssembly App ersetzen.

**Why:** Einheitliche C#-Sprache über Frontend und Backend, geteilte Modelle, sauberere MSAL-Auth-Integration, kein JS/TS-Toolchain-Overhead.

**How to apply:** Bei Implementierungsentscheidungen Blazor-Äquivalente bevorzugen; Shared-Projekt für DTOs vorschlagen.

---

## Aktueller Vue-Client

- **Stack**: Vue 3, Quasar Framework, TypeScript, Axios
- **Kein State-Management-Framework** (kein Pinia/Vuex) – component-local mit `ref()`
- **Auth**: Azure MSAL konfiguriert aber deaktiviert (`authConfig.js`)
- **Routing**: Hash-based, 6 Routen
- **API-Base**: `https://localhost:7123/`
- **Sprache**: Deutsch (UI), TypeScript (Code)

### Komponenten
| Vue | Route | Funktion |
|---|---|---|
| KaesseliHome | `/` | Dashboard: Kontosalden, Budgetübersicht |
| KaesseliAccounts | `/accounts` | Konten nach Typ gruppiert |
| KaesseliAccountTable | `/accountTable/:id` | Buchungen eines Kontos |
| KaesseliTransactions | `/transactions` | Kontoauszüge + Transaktionsliste |
| KaesseliImport | `/import` | CSV/CAMT-Datei-Upload |
| KaesseliAssign | `/assign` | Transaktionen zuordnen (komplex: Split, Automation, KI-Vorschläge) |

---

## Blazor-Migrationsplan

### Projektstruktur
```
Kaesseli.Shared/        # NEU: Gemeinsame DTOs (C# Records)
Kaesseli.Client.Blazor/ # NEU: Blazor WASM (standalone, gegen bestehenden Backend)
```

### Key Mappings
| Vue/Quasar | Blazor |
|---|---|
| `ref()`, `onMounted()` | `@code {}`, `OnInitializedAsync()` |
| Vue Router | `@page "/route"`, `NavigationManager` |
| Axios | `HttpClient` via DI |
| Quasar (Material Design) | **MudBlazor** |
| LocalStorage direkt | `Blazored.LocalStorage` |
| MSAL-JS | `Microsoft.AspNetCore.Components.WebAssembly.Authentication` |

### State Management
Empfehlung: **Service-basiert** (kein Fluxor), da der Vue-Client auch kein Store hatte:
```csharp
public class AccountingPeriodState  // Singleton via DI
{
    public string? SelectedPeriodId { get; private set; }
    public event Action? OnChange;
    public void SetPeriod(string id) { SelectedPeriodId = id; OnChange?.Invoke(); }
}
```

### Besonderheiten KaesseliAssign (komplexeste Komponente)
- Ctrl+Click für Split: `MouseEventArgs.CtrlKey`
- Live-Filter: `@oninput` + LINQ
- Debounced Automation-Counter: `Task.Delay` + `CancellationToken`
- Split-Betragsverteilung: C# `decimal` (keine JS-Floating-Point-Probleme)

### Auth
```csharp
builder.Services.AddMsalAuthentication(options =>
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication));
```
Azure-Config aus `authConfig.js` → `appsettings.json`

---

## API-Endpunkte (Backend bleibt unverändert)
- `GET /accountingPeriod` / `/{id}/overView` / `/{id}/accountSummary` / `/{id}/account/{accountId}`
- `GET /transactionSummary`, `GET /transaction?transactionSummaryId={id}`
- `GET /transaction/nextOpen`, `GET /transaction/totalOpen`
- `GET /account?accountType={id}`
- `GET /automation/nrMatchInput?input={text}`
- `POST /file/upload` (multipart), `POST /automation`
- `PATCH /transaction/journalEntry`, `PATCH /transaction/journalEntry/split`
