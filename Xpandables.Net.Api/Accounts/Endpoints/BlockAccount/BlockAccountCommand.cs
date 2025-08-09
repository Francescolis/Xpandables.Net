using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed record BlockAccountCommand : DependencyRequest<Account>, IRequiresEventStorage
{
}
