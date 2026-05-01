# Kaesseli — Testing (`Tests/Kaesseli.Test/`)

## Stack
- **xUnit 2.9** (`[Fact]`, `[Theory]`)
- **NSubstitute** für Mocks
- **Shouldly** für Assertions (`result.ShouldBe(...)`)
- **Bogus** + eigener `SmartFaker<T>` für Random-Daten
- **EF InMemory** + obsoleter parameterloser `KaesseliContext`-Ctor für DbContext-Tests
- **Microsoft.AspNetCore.TestHost** + `WebApplication.CreateBuilder(...).WebHost.UseTestServer()` für Endpoint-Integrationstests
- **Microsoft.Extensions.TimeProvider.Testing** für deterministische Zeit
- **coverlet.collector** für Coverage

## Aufbau
```
Tests/Kaesseli.Test/
├── Faker/SmartFaker.cs      Reflection-getriebener Bogus-Faker (auch private Ctors)
├── Helpers/TypeExtensions.cs
├── Infrastructure/
│   ├── ApplicationServiceCollectionExtensionsTests.cs   prüft DI-Registrierungen
│   ├── DependencyInjectionTests.cs
│   ├── ObjectExtensionsTests.cs
│   └── MockData/
└── Features/
    ├── Accounts/   (AccountApi-, Repository-, Handler-, Validation-Tests)
    ├── Automation/
    ├── Budget/
    ├── Integration/  (Sample-Daten Example.camt, RawText*.yaml als EmbeddedResources)
    └── Journal/
```

## SmartFaker
`SmartFaker<T> : Bogus.Faker<T>` automatisiert Test-Daten-Generierung:
- Findet privaten parameterlosen Ctor und ruft ihn via Reflection auf (passt zu Domain-Klassen wie `Account`, `JournalEntry` mit `private Account() {}`).
- Fallback: erster Konstruktor mit Param-Werten aus `GetValueByType(...)`.
- Setzt anschließend alle Properties mit `SetMethod` über Reflection.
- Komplexe Typen werden rekursiv populated.
- Spezielles `LongToGuid`-Mapping für reproduzierbare Guids aus Long.

Wenn ein Test komische Werte produziert: vermutlich greift einer der Reflection-Pfade nicht — Properties prüfen.

## Convention für Handler-Tests
1. Mocks per `Substitute.For<I...>()`.
2. Optional `SmartFaker` für Domain-Objekte.
3. `Handler` direkt mit Mocks instanziieren.
4. `Handle(query, CancellationToken.None)` aufrufen.
5. Mit `await mock.Received().XYZ(Arg.Is<...>(...))` Aufruf prüfen.

Beispiel: `Tests/Kaesseli.Test/Features/Accounts/AddAccountCommandHandlerTests.cs`.

## Convention für API-Tests
- `WebApplication.CreateBuilder().WebHost.UseTestServer()`
- Routing + alle benötigten Handler-Mocks als `AddSingleton(...)` registrieren
- `app.MapXEndpoints()`, `app.StartAsync()`
- HTTP-Requests gegen `app.GetTestClient()` schicken
- Status + Bodies mit Shouldly assertieren

Beispiel: `Tests/Kaesseli.Test/Features/Integration/IntegrationApiTests.cs`. (Multipart-Upload-Test ist dort als Vorlage.)

## EmbeddedResources
CAMT-XML und CSV/YAML-Beispiele liegen in `Features/Integration/ExampleData/` als Embedded Resources. Logische Namen:
- `Kaesseli.Test.Features.Integration.ExampleData.Example.camt`
- `Kaesseli.Test.Features.Integration.ExampleData.RawText1.yaml`
- `Kaesseli.Test.Features.Integration.ExampleData.RawText2.yaml`

## InternalsVisibleTo
`Kaesseli/Kaesseli.csproj` enthält `InternalsVisibleTo("Kaesseli.Test")`, damit `internal class Repository`-Implementierungen direkt instanziiert werden können.

## Tipps
- Für Tests mit `KaesseliContext`: parameterlosen Ctor nutzen + `Substitute.For<DbSet<T>>()` oder EF InMemory.
- `TimeProvider`: `FakeTimeProvider` (aus `Microsoft.Extensions.TimeProvider.Testing`) statt `TimeProvider.System`.
- Domain-Konstruktoren validieren — Tests mit `Should.Throw<TException>()` für Argument-Validation.
