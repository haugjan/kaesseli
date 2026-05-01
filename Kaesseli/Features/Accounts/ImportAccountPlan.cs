using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kaesseli.Features.Accounts;

public static class ImportAccountPlan
{
    public record Query(string Yaml);

    public record Result(int Created, int Updated);

    public interface IHandler
    {
        Task<Result> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IAccountRepository repo) : IHandler
    {
        private static readonly IDeserializer Deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();

        public async Task<Result> Handle(Query request, CancellationToken cancellationToken)
        {
            List<AccountPlanEntry> entries;
            try
            {
                entries =
                    Deserializer.Deserialize<List<AccountPlanEntry>>(request.Yaml ?? string.Empty)
                    ?? new List<AccountPlanEntry>();
            }
            catch (Exception ex)
            {
                throw new AccountPlanImportException(
                    $"YAML konnte nicht gelesen werden: {ex.Message}"
                );
            }

            ValidateNoDuplicatesWithinImport(entries);

            var existingAccounts = (await repo.GetAccounts(cancellationToken)).ToList();
            var byShortName = existingAccounts.ToDictionary(a => a.ShortName);
            var byNumber = existingAccounts.ToDictionary(a => a.Number);

            var toUpdate = new List<(Account existing, AccountPlanEntry entry)>();
            var toCreate = new List<AccountPlanEntry>();

            foreach (var entry in entries)
            {
                if (byShortName.TryGetValue(entry.ShortName, out var existing))
                {
                    if (existing.Type != entry.Type)
                        throw new AccountPlanImportException(
                            $"Konto mit Kurzbezeichnung '{entry.ShortName}' existiert bereits mit Typ {existing.Type}, "
                                + $"Import erwartet Typ {entry.Type}."
                        );

                    if (
                        byNumber.TryGetValue(entry.Number, out var numberHolder)
                        && numberHolder.Id != existing.Id
                    )
                        throw new AccountPlanImportException(
                            $"Kontonummer '{entry.Number}' wird bereits von Konto '{numberHolder.ShortName}' verwendet."
                        );

                    toUpdate.Add((existing, entry));
                }
                else
                {
                    if (byNumber.TryGetValue(entry.Number, out var numberHolder))
                        throw new AccountPlanImportException(
                            $"Kontonummer '{entry.Number}' wird bereits von Konto '{numberHolder.ShortName}' verwendet."
                        );

                    toCreate.Add(entry);
                }
            }

            // Validation phase complete; apply mutations
            foreach (var (existing, entry) in toUpdate)
            {
                existing.Update(
                    entry.Name,
                    entry.Type,
                    entry.Number,
                    entry.ShortName,
                    new AccountIcon(entry.Icon, entry.IconColor)
                );
                await repo.UpdateAccount(existing, cancellationToken);
            }

            foreach (var entry in toCreate)
            {
                var account = Account.Create(
                    entry.Name,
                    entry.Type,
                    entry.Number,
                    entry.ShortName,
                    new AccountIcon(entry.Icon, entry.IconColor)
                );
                await repo.AddAccount(account, cancellationToken);
            }

            return new Result(Created: toCreate.Count, Updated: toUpdate.Count);
        }

        private static void ValidateNoDuplicatesWithinImport(List<AccountPlanEntry> entries)
        {
            var dupShort = entries
                .GroupBy(e => e.ShortName)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (dupShort.Count > 0)
                throw new AccountPlanImportException(
                    $"Kurzbezeichnung mehrfach im Import: {string.Join(", ", dupShort)}"
                );

            var dupNumber = entries
                .GroupBy(e => e.Number)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();
            if (dupNumber.Count > 0)
                throw new AccountPlanImportException(
                    $"Kontonummer mehrfach im Import: {string.Join(", ", dupNumber)}"
                );
        }
    }
}
