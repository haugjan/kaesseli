# Kaesseli — Blazor Client (`Kaesseli.Client.Blazor/`)

Blazor WebAssembly (.NET 10) mit MudBlazor 9.x. Wird vom Backend mitgehosted (`UseBlazorFrameworkFiles`).

## Projekt-Setup
- `Kaesseli.Client.Blazor.csproj` — Blazor WASM SDK, `OverrideHtmlAssetPlaceholders`, MudBlazor, ProjectReference auf `Kaesseli.Contracts`.
- Globale Imports in `_Imports.razor`: u.a. `Kaesseli.Client.Blazor.Layout`, `Kaesseli.Contracts.Accounts/Integration/Budget`, `MudBlazor`, `Kaesseli.Client.Blazor.Components`.
- `wwwroot/index.html` — Lädt Inter-Font (Google Fonts), MudBlazor-CSS/JS, App-CSS, `_framework/blazor.webassembly.js`. Kein MSAL-Skript (per Branch entfernt).
- `wwwroot/appsettings.json` — `ApiBaseUrl` + `BasicAuth.Username` + `BasicAuth.Password`. Achtung: Das ist im veröffentlichten Bundle public einsehbar — Basic Auth ist kein Schutz vor entschlossenen Nutzern, sondern ein einfaches Gate.

## `Program.cs`
- Liest `ApiBaseUrl` (Pflicht) und `BasicAuth:Username|Password` aus Konfiguration.
- Baut einen `HttpClient` mit `BaseAddress` und setzt `DefaultRequestHeaders.Authorization` auf `Basic <base64>` (sofern Credentials vorhanden) — das spart das ständige Browser-Prompt.
- Registriert `KaesseliApiService` (scoped), `AccountingPeriodState` (Singleton), `AddMudServices`.
- Setzt `CultureInfo` global aus `navigator.language` via JS-Interop (für Datums-/Zahlenformate).

## API-Layer: `Services/KaesseliApiService.cs`
Eine zentrale Klasse, die alle Backend-Endpoints kapselt (`HttpClient`-basiert). Wichtige Methoden:
- `GetAccountingPeriodsAsync`, `GetOverviewAsync`, `GetAccountSummariesAsync`, `GetAccountStatementAsync`
- `GetTransactionSummariesAsync`, `GetTransactionsAsync`, `GetNextOpenTransactionAsync`
- `AssignTransactionAsync`, `SplitTransactionAsync`
- `AddAutomationAsync`, `GetNrOfPossibleAutomationsAsync`
- Account- & AccountingPeriod-CRUD (`AddAccountAsync` etc.)
- Budget: `GetBudgetEntriesAsync`, `SetBudgetAsync`
- Journal: `GetJournalEntriesAsync`, `AddOpeningBalanceAsync`
- File-Upload: `UploadFileAsync` (Multipart)
- Lokale Records: `SplitEntry(Guid OtherAccountId, decimal Amount)`, `NrOfPossibleAutomationResult` (privat).

## State: `Services/AccountingPeriodState.cs`
- Singleton, hält `Periods` + `SelectedPeriodId`.
- `Initialize(periods, savedId)` — wählt gespeicherte Period oder Letzte.
- `SelectPeriod(id)` — feuert `OnChange`.
- Komponenten subscriben in `OnInitializedAsync` und entladen via `IDisposable`.

## Layout
- `Layout/MainLayout.razor` — `MudLayout` mit `MudAppBar` (Period-Selector, DarkMode-Toggle), `MudDrawer` (NavMenu), `MudMainContent`. Lädt beim Start die Perioden + Saved-Id (LocalStorage `selectedPeriod`) und ruft `PeriodState.Initialize`.
- `Layout/NavMenu.razor` — Routen: `/` (Home), `/accounts`, `/assign`, `/import`, `/transactions`, `/settings/periods`, `/settings/accounts`. UI komplett deutsch ("Konten", "Zuweisen", "Kontoauszüge", "Einstellungen").
- `Theme/` — Custom MudTheme (`KaesseliTheme.Create()`).

## Pages (`Pages/*.razor`)
| Route | File | Zweck |
|---|---|---|
| `/` | Home.razor | Dashboard mit drei `OverviewCard`s (Einkommen, Ausgaben, Vermögen) |
| `/accounts` | Accounts.razor | Konten gruppiert nach `AccountType`, Tabelle mit Saldo + Budget |
| `/accountTable/{...}` | AccountTable.razor | Buchungen eines einzelnen Kontos |
| `/accountingPeriod/{...}` | AccountingPeriodDetail.razor | Detailansicht einer Periode |
| `/assign` | Assign.razor | Komplexe Zuordnung offener Transaktionen (Split, Automation, Filter) |
| `/import` | Import.razor | Datei-Upload (CSV/CAMT/ZIP) |
| `/transactions` | Transactions.razor | Kontoauszüge / Transaktions-Liste |
| `/settings/accounts` | SettingsAccounts.razor | Konten verwalten |
| `/settings/periods` | AccountingPeriods.razor | Perioden verwalten |
| `*` | NotFound.razor | 404 |

`Counter.razor` ist wahrscheinlich ein Template-Überrest; nicht im NavMenu verlinkt.

## Components (`Components/*.razor`)
- `OverviewCard` — wiederverwendbare Karte (Icon, Summary, optional Budget).
- `AccountDialog`, `AccountingPeriodDialog`, `DeleteConfirmationDialog` — Modal-Dialoge (MudBlazor).

## Verhaltens-Patterns
- Pages mit `OnInitializedAsync` → `PeriodState.OnChange += handler`, in `Dispose()` abmelden.
- `MudSkeleton` als Loading-Placeholder solange Daten `null`.
- Lokalisierung: alle UI-Labels deutsch, Datumsformat über `dddd, dd. MMMM yyyy` (CultureInfo aus Browser).
- Currency-Formatierung über `decimal.ToString("N2")` / Custom Helper.

## Beziehung zum Vue-Client
`Kaesseli.Client/` (Vue/Quasar/TypeScript) ist der Legacy-Client und wird durch den Blazor-Client abgelöst. Status: Migration in Arbeit. Siehe `.claude/blazor_migration.md`. Beim Bauen von Features den Blazor-Client als Wahrheit nehmen, der Vue-Client wird nicht weiter ausgebaut.
