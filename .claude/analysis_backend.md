# Kaesseli — Backend (`Kaesseli/`)

ASP.NET Core (.NET 10) Minimal-API + EF Cosmos. Vertical-Slice-Style nach Features.

## Eintritt: `Program.cs`
1. Konfiguration: `appsettings.json` + `appsettings.{env}.json` + `appsettings.user.json` (lokal) + Azure KeyVault (wenn `KeyVault:VaultUri` gesetzt).
2. OpenAPI (Scalar UI in Dev unter `/scalar`, beide `AllowAnonymous`).
3. Health-Check unter `/healthz` (`AllowAnonymous`).
4. **OpenTelemetry — opt-in**: nur registriert, wenn `Otlp:Endpoint` gesetzt ist (Dev-Default `http://localhost:4317`, Header `x-otlp-api-key=<Otlp:ApiKey || "kaesseli-dev">`). Verhindert gRPC-Verbindungsversuche in Prod.
5. CORS-Policy `AllowSpecificOrigin` für 7033/9000/9500/9501 (alle Methoden/Header).
6. **Auth: `AddGoogleAuth(...)`** (siehe unten).
7. `AddInfrastructureServices(...)` und `AddApplicationServices()`.
8. Pipeline: `UseHttpsRedirection` → CORS → `UseBlazorFrameworkFiles` + `UseDefaultFiles` + `UseStaticFiles` → `UseAuthentication` → `UseAuthorization`.
9. **DB-Init im Background-Task** (`Task.Run` → `KaesseliContext.Database.EnsureCreatedAsync()`). Blockiert den Startup nicht — Cosmos braucht beim Cold-Start mehrere Round-Trips.
10. In Dev: Browser-Auto-Open für Aspire-Dashboard auf `http://localhost:18888`.
11. `MapGoogleAuthProxy()` (siehe unten) und `MapKaesseliEndpoints()`.
12. `MapFallbackToFile("/index.html").AllowAnonymous()` — der SPA-Bootstrap muss anonym ladbar sein, der Client läuft danach in den OIDC-Flow.

## Auth: `GoogleAuth.cs`
- **JWT Bearer gegen Google**: `Authority = https://accounts.google.com`, `Audience = Auth:Google:ClientId`, `NameClaimType = "email"`, `MapInboundClaims = false`.
- **Email-Allowlist**: `EmailAllowlistRequirement` + `EmailAllowlistHandler` prüfen Token-Claim `email_verified == "true"` und ob das `email`-Claim in `Auth:Google:AllowedEmails` (Array) steht. Leere Allowlist → niemand wird authorized (Warning-Log).
- **Default-Policy**: `SetFallbackPolicy(RequireAuthenticatedUser + EmailAllowlistRequirement)` — schützt **jede** Route automatisch. Wer anonym sein soll, muss `.AllowAnonymous()` setzen.
- **Token-Proxy** unter `/auth/google/token` (POST, anonym, `DisableAntiforgery`): nimmt das vom Client geschickte OIDC-Token-Request-Body, hängt das `Auth:Google:ClientSecret` aus der Backend-Config dran und ruft `https://oauth2.googleapis.com/token` auf. Damit muss das ClientSecret nicht ins Public-WASM-Bundle.
- **Metadata-Proxy** unter `/auth/google/.well-known/openid-configuration` (GET, anonym): lädt die echte Google-OIDC-Metadata, ersetzt `token_endpoint` durch `<origin>/auth/google/token` (Origin aus `X-Forwarded-Proto` oder `Request.Scheme`).

> Basic Auth / `BasicAuthHandler.cs` und MSAL/Azure B2C sind raus. Falls noch Pakete in `Directory.Packages.props` stehen: nur Altlast.

## Endpoint-Mapping
`Infrastructure/EndpointRouteBuilderExtensions.cs::MapKaesseliEndpoints()` ruft sequenziell auf:
- `MapBudgetEndpoints` (Features/Budget/BudgetApi.cs)
- `MapJournalEndpoints` (Features/Journal/JournalEntryApi.cs)
- `MapAccountEndpoints` (Features/Accounts/AccountApi.cs)
- `MapIntegrationEndpoints` (Features/Integration/IntegrationApi.cs)
- `MapAutomationEndpoints` (Features/Automation/AutomationApi.cs)

Convention: jede `*Api.cs` ist ein `extension(IEndpointRouteBuilder app)` mit `MapXEndpoints()`. Routen sind plain Minimal-API; Handler werden als typed Interfaces aus DI geholt.

### Endpoint-Übersicht
| Methode | Route | Handler |
|---|---|---|
| GET | `/healthz` | (Health-Check, anonym) |
| GET | `/auth/google/.well-known/openid-configuration` | OIDC-Metadata-Proxy (anonym) |
| POST | `/auth/google/token` | Token-Proxy (anonym) |
| GET | `/account?accountType=` | `GetAccounts` |
| GET | `/account/plan` | `ExportAccountPlan` (YAML-Download) |
| POST | `/account/plan` | `ImportAccountPlan` (Body: `application/x-yaml`) |
| GET | `/accountingPeriod` | `GetAccountingPeriods` |
| GET | `/accountingPeriod/{accountingPeriodId}/account/{accountId}` | `GetAccount` |
| GET | `/accountingPeriod/{accountingPeriodId}/accountSummary` | `GetAccountsSummary` |
| GET | `/accountingPeriod/{accountingPeriodId}/overView` | `GetFinancialOverview` |
| POST | `/account` | `AddAccount` |
| PUT | `/account/{id}` | `UpdateAccount` |
| DELETE | `/account/{id}` | `DeleteAccount` (409 Conflict bei `AccountInUseException`) |
| POST | `/accountingPeriod` | `AddAccountingPeriod` |
| PUT | `/accountingPeriod/{id}` | `UpdateAccountingPeriod` |
| DELETE | `/accountingPeriod/{id}` | `DeleteAccountingPeriod` |
| POST | `/admin/cleanupOrphanedAccountReferences` | `CleanupOrphanedAccountReferences` |
| POST | `/budgetEntry` | `SetBudget` |
| GET | `/budgetEntry?accountingPeriodId=&accountId=&accountType=` | `GetBudgetEntries` |
| POST | `/journalEntry` | `AddJournalEntry` |
| POST | `/journalEntry/openingBalance` | `AddOpeningBalance` |
| GET | `/journalEntry?accountingPeriodId=&accountId=&accountType=` | `GetJournalEntries` |
| GET | `/transactionSummary` | `GetTransactionSummaries` |
| GET | `/transaction?transactionSummaryId=` | `GetTransactions` |
| GET | `/transaction/nextOpen?skip=` | `GetNextOpenTransaction` |
| GET | `/transaction/totalOpen` | `GetTotalOpenTransaction` |
| PATCH | `/transaction/journalEntry` | `AssignOpenTransaction` |
| PATCH | `/transaction/journalEntry/split` | `SplitOpenTransaction` |
| POST | `/file/upload` (multipart) | `ProcessFile` (CSV / CAMT, ZIP wird intern entpackt) |
| GET | `/automation/nrMatchInput?input=` | `GetNrOfPossibleAutomation` |
| POST | `/automation` | `AddAutomation` (intern: `ApplyAllAutomations`) |

## Feature-Slice-Konvention
Jeder Use-Case ist ein eigener `static class` mit:
```csharp
public static class XAction {
    public record Query(...);                          // Request-DTO
    public interface IHandler { Task<TResult> Handle(Query, CancellationToken); }
    public class Handler(deps...) : IHandler { ... }   // primary ctor
}
```
Vorteile: Endpoint registriert nur `IHandler`, Tests mocken `IHandler`. Registrierung in `ApplicationServiceCollectionExtensions.AddApplicationServices()` (transient).

## Domänen-Modell
Klassen mit `private` Ctor + statischer `Create(...)`-Factory + `Update(...)`. Beispiele: `Account`, `AccountingPeriod`, `JournalEntry` (mit zusätzlicher `CreateOpeningBalance`), `Transaction`, `TransactionSummary`, `BudgetEntry`, `AutomationEntry`/`AutomationEntryPart`. Keine Datenannotationen, alle Constraints in den Factories (Argument-Validation + Domain-Exceptions wie `AccountsMustNotBeSameException`, `BudgetNotAllowedException`, `WrongAmountException`, `AccountInUseException`, `DuplicateAccountNumberException`, `DuplicateAccountShortNameException`, `InvalidAccountNumberException`, `InvalidAccountShortNameException`, `AccountNumberDoesNotMatchTypeException`, `AccountPlanImportException`, `AutomationAccountShortNameNotFoundException`).

## Persistenz: `Infrastructure/KaesseliContext.cs`
- `DbContext` mit `DbSet<>` für: `JournalEntries`, `BudgetEntries`, `Accounts`, `TransactionSummaries`, `Transactions`, `AccountingPeriods`, `TransactionStatistics`, `Automations`, `PaymentEntries`.
- Cosmos-PartitionKey ist überall `Id` (Single-Document-Partition).
- `OnModelCreating`: Owned Types für `Account.Icon`, Eltern-Kind für `AutomationEntry.Parts`, `BudgetEntry → Account/AccountingPeriod`, `JournalEntry → DebitAccount/CreditAccount/Transaction`, `TransactionSummary → Account` und `Transactions`.
- Shadow-Properties `InsertDate/InsertUser/EditDate/EditUser` für jede Entität (außer Owned). Werte werden in `SaveChangesAsync` aus `TimeProvider` und `Environment.UserName` gesetzt.
- Obsoleter parameterloser Ctor existiert nur für Unit-Tests (Mocking).

## DI-Setup: `Infrastructure/InfrastructureServiceCollectionExtensions.cs`
- `AddRepositories` (alle scoped):
  - `IBudgetRepository`, `IJournalRepository`, `IAccountRepository`, `IAutomationRepository`, `ITransactionRepository`
- Scoped-Services: `ICamtProcessor`, `IPostFinanceCsvProcessor`
- `AddDbContext<KaesseliContext>` mit Cosmos-Optionen:
  - Endpoint/Key/Database aus Config (Pflicht, sonst InvalidOperation).
  - Lokal-Emulator (`endpoint contains "localhost"`): Gateway-Mode + selbstsigniertes Zertifikat ignorieren.
  - Sonst: Region "Switzerland North".

> CORS, Google-Auth und der Application-Layer werden in `Program.cs` registriert (`AddCors`, `AddGoogleAuth`, `AddApplicationServices`), nicht in `AddInfrastructureServices`.

## Konfiguration
- `appsettings.json` — produktive Defaults (`CosmosDb:Endpoint` zeigt auf Azure-Account, `Auth:Google:ClientId`/`AllowedEmails` leer)
- `appsettings.Development.json` — Override `CosmosDb:Database = dev-kaesseli`, `Otlp:Endpoint = http://localhost:4317`
- `appsettings.user.json` — lokal nur (Geheimnisse: `CosmosDb:Key`, `Auth:Google:ClientSecret`, evtl. `Auth:Google:ClientId` und `AllowedEmails`); nicht in publish
- KeyVault wird bei gesetzem `KeyVault:VaultUri` automatisch via `DefaultAzureCredential` eingehängt (für Prod: `Auth:Google:ClientSecret` und `CosmosDb:Key`).

### Wichtige Config-Keys
- `Auth:Google:ClientId` — Google OAuth Client ID (Web Application)
- `Auth:Google:ClientSecret` — bleibt serverseitig (KeyVault / `appsettings.user.json`)
- `Auth:Google:AllowedEmails` — Array von erlaubten E-Mail-Adressen
- `CosmosDb:Endpoint` / `CosmosDb:Key` / `CosmosDb:Database`
- `Otlp:Endpoint` (optional) / `Otlp:ApiKey` (optional, default `kaesseli-dev`)
- `KeyVault:VaultUri` (optional)

## Files / Subordner
- `Features/Accounts/` — Account + AccountingPeriod CRUD, Salden-Berechnung (`AccountBalanceCalculator`), Übersichten, **Kontoplan-YAML-Export/Import** (`ExportAccountPlan`, `ImportAccountPlan`, `AccountPlanEntry`), **Cleanup verwaister Account-Referenzen** (`CleanupOrphanedAccountReferences`)
- `Features/Automation/` — Pattern-basierte Automatisierung von Transaktionszuordnung (`AutomationEntry`, `AutomationEntryPart`, `ApplyAllAutomations`)
- `Features/Budget/` — Budget setzen/abfragen (nur Revenue/Expense erlaubt — `BudgetNotAllowedException`)
- `Features/Integration/FileImport/` — `ICamtProcessor` (XML-Schemas in `Camt053Schema.cs`), `IPostFinanceCsvProcessor` (CsvHelper). `ProcessFile.Handler` dispatched anhand `FileType` an `ProcessCamtFile` oder `ProcessPostFinanceCsv`.
- `Features/Integration/NextOpenTransaction/` — Workflow für offene Transaktionen (Get, Assign, Split, Total, `OpenTransactionAmountChanged`-Event)
- `Features/Integration/TransactionQuery/` — Lese-Queries
- `Features/Journal/` — Journaleinträge & Eröffnungsbilanz, Validierungen (`AccountsMustNotBeSameException`, `WrongAmountException`)
- `Infrastructure/` — DbContext, Endpoint-Mapping, Service-Registrierung, `EntityNotFoundException`, kleine Extensions
- `GoogleAuth.cs` — Auth-Setup, Allowlist-Handler, OIDC-Proxies (im Root, nicht in `Infrastructure/`)

## Konventionen / Tipps
- Globale Aliasse in `GlobalUsings.cs`: `AccountType = Kaesseli.Contracts.Accounts.AccountType` und `Contracts = Kaesseli.Contracts`.
- Endpoints geben rohe DTOs/Records zurück (kein automatisches Mapping-Framework). Das Backend-`Account` und der Contract `Account` sind getrennt; Mapping wird wo nötig manuell gemacht.
- `EnsureCreatedAsync` ist die ganze Migration. Es gibt **keine** EF-Migrations.
- Kein MediatR — die Handler-Interfaces sind selbst gebaut, eine `IHandler.Handle` pro Slice.
- Primary-Constructors überall (`class Handler(IDep d) : IHandler`).
- Neue Endpoints sind per Default geschützt; nur explizit `.AllowAnonymous()` setzen, wenn wirklich gewollt (Health, OIDC-Proxies, SPA-Fallback).
