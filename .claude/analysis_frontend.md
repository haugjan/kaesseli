# Kaesseli — Blazor Client (`Kaesseli.Client.Blazor/`)

Blazor WebAssembly (.NET 10) mit MudBlazor 9.x. Wird vom Backend mitgehosted (`UseBlazorFrameworkFiles`). Als PWA installierbar.

## Projekt-Setup
- `Kaesseli.Client.Blazor.csproj` — Blazor WASM SDK, `OverrideHtmlAssetPlaceholders`, MudBlazor, `Microsoft.AspNetCore.Components.WebAssembly.Authentication`, ProjectReference auf `Kaesseli.Contracts`.
- Globale Imports in `_Imports.razor`: u.a. `Kaesseli.Client.Blazor.Layout`, `Kaesseli.Contracts.Accounts/Integration/Budget`, `MudBlazor`, `Kaesseli.Client.Blazor.Components`, Authentication-Namespaces.
- `wwwroot/index.html` — Lädt Inter-Font (Google Fonts), MudBlazor-CSS/JS, App-CSS, `AuthenticationService.js` (für OIDC-Flow), `_framework/blazor.webassembly.js`. Inline-Skript registriert `window.kaesseli.downloadText(...)` (für YAML-Export) und `window.kaesseliAuth.getIdToken()` (liest das Google-ID-Token aus `sessionStorage` für den Bearer-Header). Service-Worker wird registriert (PWA).
- `wwwroot/manifest.webmanifest` + `icon-192/512.png` + `apple-touch-icon` — PWA-Setup.
- `wwwroot/appsettings.json` — nur **`Auth:Google:ClientId`** + Redirect-URIs. **Kein Secret und keine User-Credentials** im Bundle. Die API-BaseAddress kommt aus `builder.HostEnvironment.BaseAddress` (Backend hostet den Client, gleicher Origin).

## `Program.cs`
- `AddOidcAuthentication` mit Google als Authority:
  - `Authority = https://accounts.google.com`
  - `MetadataUrl = <baseAddress>/auth/google/.well-known/openid-configuration` (zeigt auf den Backend-Proxy, der `token_endpoint` auf `<baseAddress>/auth/google/token` umbiegt)
  - `ResponseType = "code"`, Scopes `openid email profile`
  - `RedirectUri = /authentication/login-callback`, `PostLogoutRedirectUri = /authentication/logout-callback`
  - `UserOptions.NameClaim = "email"`
- **`IdTokenAuthorizationMessageHandler`** (DelegatingHandler) — holt das ID-Token via JS-Interop (`kaesseliAuth.getIdToken`) aus `sessionStorage` (Key-Prefix `oidc.user:`) und setzt es als `Authorization: Bearer` auf jede Anfrage.
- `HttpClient` mit `BaseAddress = builder.HostEnvironment.BaseAddress` und obigem Handler — alle API-Calls gehen relativ.
- Registriert `KaesseliApiService` (scoped), `AccountingPeriodState` (Singleton), `AddMudServices`.
- Setzt `CultureInfo` global aus `navigator.language` via JS-Interop (für Datums-/Zahlenformate).

## API-Layer: `Services/KaesseliApiService.cs`
Eine zentrale Klasse, die alle Backend-Endpoints kapselt (`HttpClient`-basiert). Wichtige Methoden:
- `GetAccountingPeriodsAsync`, `GetOverviewAsync`, `GetAccountSummariesAsync`, `GetAccountStatementAsync`
- `GetTransactionSummariesAsync`, `GetTransactionsAsync`, `GetNextOpenTransactionAsync`
- `AssignTransactionAsync`, `SplitTransactionAsync`
- `AddAutomationAsync`, `GetNrOfPossibleAutomationsAsync`
- Account- & AccountingPeriod-CRUD (`AddAccountAsync`, `UpdateAccountAsync`, `DeleteAccountAsync` — wirft `AccountInUseClientException` bei HTTP 409)
- Account-Plan: `ExportAccountPlanAsync` (YAML-Download), `ImportAccountPlanAsync`
- Wartung: `CleanupOrphanedAccountReferencesAsync` → `CleanupOrphansResult`
- Budget: `GetBudgetEntriesAsync`, `SetBudgetAsync`
- Journal: `GetJournalEntriesAsync`, `AddOpeningBalanceAsync`
- File-Upload: `UploadFileAsync` (Multipart)
- Lokale Records: `SplitEntry(Guid OtherAccountId, decimal Amount)`, `NrOfPossibleAutomationResult` (privat).

## State: `Services/AccountingPeriodState.cs`
- Singleton, hält `Periods` + `SelectedPeriodId`.
- `Initialize(periods, savedId)` — wählt gespeicherte Period oder Letzte.
- `SelectPeriod(id)` — feuert `OnChange`.
- Komponenten subscriben in `OnInitializedAsync` und entladen via `IDisposable`.

## Auth-Flow (Client-Sicht)
- `App.razor` ist in `<CascadingAuthenticationState>` gewrapped, jede Route nutzt `AuthorizeRouteView`. Nicht-authentifizierte User werden via `<RedirectToLogin />` zum Login geschickt.
- `Pages/Authentication.razor` (`@page "/authentication/{action}"`, `@layout EmptyLayout`, `[AllowAnonymous]`) hostet `<RemoteAuthenticatorView>` für `login-callback`/`logout-callback`/etc.
- `Layout/EmptyLayout.razor` ist ein minimaler Layout für die Auth-Routen (kein AppBar/Drawer).
- `Components/LoginDisplay.razor` zeigt Email + Logout-Button (authentifiziert) bzw. Login-Button (anonym), nutzt `NavigationManager.NavigateToLogin/Logout`.
- `Components/RedirectToLogin.razor` — leitet auf `authentication/login` mit `returnUrl` weiter.
- ID-Token landet in `sessionStorage` (OIDC-Library); `IdTokenAuthorizationMessageHandler` liest's heraus und setzt den Bearer-Header.

## Layout
- `Layout/MainLayout.razor` — `MudLayout` mit `MudAppBar` (Period-Selector, DarkMode-Toggle, `<LoginDisplay />`), `MudDrawer` (NavMenu), `MudMainContent`.
  - Lädt Perioden + Saved-Id (LocalStorage `selectedPeriod`) erst nach `AuthenticationStateProvider.GetAuthenticationStateAsync()` → ruft `PeriodState.Initialize`.
  - **DEV-Banner**: gelber `dev-badge` + gelber AppBar-Hintergrund, wenn `Navigation.BaseUri` den Host-Substring `-dev` enthält (Commit `81c983a Visually mark the dev environment in the AppBar`).
- `Layout/NavMenu.razor` — Routen: `/` (Home), `/accounts`, `/assign`, `/import`, `/transactions`, `/settings/periods`, `/settings/accounts`. UI komplett deutsch ("Konten", "Zuweisen", "Kontoauszüge", "Einstellungen"). `AdminCleanupOrphans` ist nicht im Menü — direkt über `/admin/cleanup-orphans` aufrufen.
- `Theme/` — Custom MudTheme (`KaesseliTheme.Create()`).

## Pages (`Pages/*.razor`)
| Route | File | Zweck |
|---|---|---|
| `/` | Home.razor | Dashboard mit drei `OverviewCard`s (Einkommen, Ausgaben, Vermögen) |
| `/accounts` | Accounts.razor | Konten gruppiert nach `AccountType`, Tabelle mit Saldo + Budget; Mobile-Layout kompakt |
| `/accountTable/{...}` | AccountTable.razor | Buchungen eines einzelnen Kontos (Mobile: Compact-Liste) |
| `/accountingPeriod/{...}` | AccountingPeriodDetail.razor | Detailansicht einer Periode |
| `/assign` | Assign.razor | Komplexe Zuordnung offener Transaktionen (Split, Automation, Filter) |
| `/import` | Import.razor | Datei-Upload (CSV/CAMT/ZIP) |
| `/transactions` | Transactions.razor | Kontoauszüge / Transaktions-Liste |
| `/settings/accounts` | SettingsAccounts.razor | Konten verwalten + YAML-Export/Import des Kontoplans |
| `/settings/periods` | AccountingPeriods.razor | Perioden verwalten |
| `/admin/cleanup-orphans` | AdminCleanupOrphans.razor | Wartung: verwaiste Buchungen/Budgets bereinigen (nicht im NavMenu) |
| `/authentication/{action}` | Authentication.razor | OIDC-Login/Logout-Callbacks (anonym, EmptyLayout) |
| `*` | NotFound.razor | 404 |

`Counter.razor` ist ein Template-Überrest; nicht im NavMenu verlinkt.

## Components (`Components/*.razor`)
- `OverviewCard` — wiederverwendbare Karte (Icon, Summary, optional Budget).
- `AccountDialog`, `AccountingPeriodDialog` — Modal-Dialoge (MudBlazor).
- `DeleteConfirmationDialog` — verlangt das Eintippen des Entity-Namens zur Bestätigung (Commit `4720efc Require typing the entity name to confirm deletion`).
- `LoginDisplay` — User-Email + Logout-Button im AppBar.
- `RedirectToLogin` — Redirect-Helper für `<NotAuthorized>` in `App.razor`.

## Verhaltens-Patterns
- Pages mit `OnInitializedAsync` → `PeriodState.OnChange += handler`, in `Dispose()` abmelden.
- `MudSkeleton` als Loading-Placeholder solange Daten `null`.
- Lokalisierung: alle UI-Labels deutsch, Datumsformat über `dddd, dd. MMMM yyyy` (CultureInfo aus Browser).
- Currency-Formatierung über `decimal.ToString("N2")` / Custom Helper.
- Alle Routes erfordern Authentifizierung (`AuthorizeRouteView` in `App.razor`); explizit anonym nur `Pages/Authentication.razor`.

## Beziehung zum Vue-Client
`Kaesseli.Client/` (Vue/Quasar/TypeScript) ist der Legacy-Client und wurde durch den Blazor-Client ersetzt. Er wird nicht mehr ausgebaut und ist nicht im Docker-Image. Beim Bauen von Features den Blazor-Client als Wahrheit nehmen.
