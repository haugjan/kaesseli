using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Kaesseli.Features.Accounts;

public static class ExportAccountPlan
{
    public interface IHandler
    {
        Task<string> Handle(CancellationToken cancellationToken);
    }

    public class Handler(IAccountRepository repo) : IHandler
    {
        private static readonly ISerializer Serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        public async Task<string> Handle(CancellationToken cancellationToken)
        {
            var accounts = await repo.GetAccounts(cancellationToken);
            var entries = accounts
                .OrderBy(a => a.Number)
                .Select(a => new AccountPlanEntry
                {
                    Number = a.Number,
                    ShortName = a.ShortName,
                    Name = a.Name,
                    Type = a.Type,
                    Icon = a.Icon.Name,
                    IconColor = a.Icon.Color,
                })
                .ToList();

            return Serializer.Serialize(entries);
        }
    }
}
