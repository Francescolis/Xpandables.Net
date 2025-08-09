using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed record UnBlockAccountCommand : DependencyRequest<Account>, IRequiresEventStorage
{
}