# Kaesseli — Backend (`Kaesseli/`)

ASP.NET Core (.NET 10) Minimal-API + EF Cosmos. Vertical-Slice-Style nach Features.

## Eintritt: `Program.cs`
1. Konfiguration: `appsettings.json` + `appsettings.{env}.json` + `appsettings.user.json` (lokal) + Azure KeyVault (wenn `KeyVault:VaultUri` gesetzt).
2. OpenAPI (Scalar UI in Dev unter `/scalar`).
3. OpenTelemetry: ASP.NET Core + HttpClient + EF Core Sources → OTLP gRPC. Endpoint default `http://localhost:4317`, ApiKey `kaesseli-dev` (lokal Aspire-Dashboard, siehe `Kaesseli.Deploy/docker-compose.yml`). Auch der `ILogger` exportiert OTLP.
4. CORS-Policy `AllowSpecificOrigin` für 7033/9000/9500/9501 (alle Methoden/Header).
5. Auth: nur Basic-Scheme registriert (`BasicAuthHandler`). Nach `UseAuthentication` wird im Pipeline-Middleware-Step `ChallengeAsync("Basic")` aufgerufen, sobald `User.Identity.IsAuthenticated == false` — damit verlangt **jede** Route 401, kein anonymer Zugriff.
6. `UseBlazorFrameworkFiles` + `UseDefaultFiles` + `UseStaticFiles` → Blazor-Client wird mitgehosted.
7. `context.Database.EnsureCreatedAsync()` beim Start (Cosmos legt Container an, falls nicht vorhanden).
8. In Dev: Browser-Auto-Open für Aspire-Dashboard auf `http://localhost:18888`.
9. `MapKaesseliEndpoints()` — siehe unten.
10. `MapFallbackToFile("/index.html")` für SPA-Routen.

## Auth: `BasicAuthHandler.cs`
- Erwartet `Authorization: Basic <base64>` Header.
- Vergleicht Plaintext gegen Config-Keys `BasicAuth:Username` / `BasicAuth:Password`.
- Sendet `WWW-Authenticate: Basic realm="Kaesseli"` bei Challenge.
- Credentials liegen für lokale Entwicklung in `Kaesseli.Client.Blazor/wwwroot/appsettings.json` (für den Client) und im KeyVault/User-Secrets für das Backend.

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
| GET | `/account?accountType=` | `GetAccounts` |
| GET | `/accountingPeriod` | `GetAccountingPeriods` |
| GET | `/accountingPeriod/{accountingPeriodId}/account/{accountId}` | `GetAccount` |
| GET | `/accountingPeriod/{accountingPeriodId}/accountSummary` | `GetAccountsSummary` |
| GET | `/accountingPeriod/{accountingPeriodId}/overView` | `GetFinancialOverview` |
| POST | `/account` | `AddAccount` |
| PUT | `/account/{id}` | `UpdateAccount` |
| DELETE | `/account/{id}` | `DeleteAccount` |
| POST | `/accountingPeriod` | `AddAccountingPeriod` |
| PUT | `/accountingPeriod/{id}` | `UpdateAccountingPeriod` |
| DELETE | `/accountingPeriod/{id}` | `DeleteAccountingPeriod` |
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
| POST | `/automation` | `AddAutomation` |

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
Klassen mit `private` Ctor + statischer `Create(...)`-Factory + `Update(...)`. Beispiele: `Account`, `AccountingPeriod`, `JournalEntry` (mit zusätzlicher `CreateOpeningBalance`), `Transaction`, `TransactionSummary`, `BudgetEntry`, `AutomationEntry`/`AutomationEntryPart`. Keine Datenannotationen, alle Constraints in den Factories (Argument-Validation + Domain-Exceptions wie `AccountsMustNotBeSameException`, `BudgetNotAllowedException`, `WrongAmountException`).

## Persistenz: `Infrastructure/KaesseliContext.cs`
- `DbContext` mit `DbSet<>` für: `JournalEntries`, `BudgetEntries`, `Accounts`, `TransactionSummaries`, `Transactions`, `AccountingPeriods`, `TransactionStatistics`, `Automations`, `PaymentEntries`.
- Cosmos-PartitionKey ist überall `Id` (Single-Document-Partition).
- `OnModelCreating`: Owned Types für `Account.Icon`, Eltern-Kind für `AutomationEntry.Parts`, `BudgetEntry → Account/AccountingPeriod`, `JournalEntry → DebitAccount/CreditAccount/Transaction`, `TransactionSummary → Account` und `Transactions`.
- Shadow-Properties `InsertDate/InsertUser/EditDate/EditUser` für jede Entität (außer Owned). Werte werden in `SaveChangesAsync` aus `TimeProvider` und `Environment.UserName` gesetzt.
- Obsoleter parameterloser Ctor existiert nur für Unit-Tests (Mocking).

## DI-Setup: `Infrastructure/InfrastructureServiceCollectionExtensions.cs`
- `AddCors`, `AddRepositories` (alle scoped):
  - `IBudgetRepository`, `IJournalRepository`, `IAccountRepository`, `IAutomationRepository`, `ITransactionRepository`
- Scoped-Services: `ICamtProcessor`, `IPostFinanceCsvProcessor`
- `AddDbContext<KaesseliContext>` mit Cosmos-Optionen:
  - Endpoint/Key/Database aus Config (Pflicht, sonst InvalidOperation).
  - Lokal-Emulator (`endpoint contains "localhost"`): Gateway-Mode + selbstsigniertes Zertifikat ignorieren.
  - Sonst: Region "Switzerland North".

## Konfiguration
- `appsettings.json` — produktive Defaults (CosmosDb-Endpoint zeigt auf Azure-Account)
- `appsettings.Development.json` — Override Database `dev-kaesseli`, OTLP localhost
- `appsettings.user.json` — lokal nur (Geheimnisse, nicht in publish)
- KeyVault wird bei gesetzem `KeyVault:VaultUri` automatisch via `DefaultAzureCredential` eingehängt.

## Files / Subordner
- `Features/Accounts/` — Account + AccountingPeriod CRUD, Salden-Berechnung (`AccountBalanceCalculator`), Übersichten
- `Features/Automation/` — Pattern-basierte Automatisierung von Transaktionszuordnung (`AutomationEntry`, `AutomationEntryPart`, `ApplyAllAutomations`)
- `Features/Budget/` — Budget setzen/abfragen (nur Revenue/Expense erlaubt — `BudgetNotAllowedException`)
- `Features/Integration/FileImport/` — `ICamtProcessor` (XML-Schemas in `Camt053Schema.cs`), `IPostFinanceCsvProcessor` (CsvHelper). `ProcessFile.Handler` dispatched anhand `FileType`.
- `Features/Integration/NextOpenTransaction/` — Workflow für offene Transaktionen (Get, Assign, Split, Total, AmountChanged-Event)
- `Features/Integration/TransactionQuery/` — Lese-Queries
- `Features/Journal/` — Journaleinträge & Eröffnungsbilanz, Validierungen (`AccountsMustNotBeSameException`, `WrongAmountException`)
- `Infrastructure/` — DbContext, Endpoint-Mapping, Service-Registrierung, Exceptions, kleine Extensions

## Konventionen / Tipps
- Globale Aliasse in `GlobalUsings.cs`: `AccountType = Kaesseli.Contracts.Accounts.AccountType` und `Contracts = Kaesseli.Contracts`.
- Endpoints geben rohe DTOs/Records zurück (kein automatisches Mapping-Framework). Das Backend-`Account` und der Contract `Account` sind getrennt; Mapping wird wo nötig manuell gemacht.
- `EnsureCreatedAsync` ist die ganze Migration. Es gibt **keine** EF-Migrations.
- Kein MediatR — die Handler-Interfaces sind selbst gebaut, eine `IHandler.Handle` pro Slice.
- Primary-Constructors überall (`class Handler(IDep d) : IHandler`).
