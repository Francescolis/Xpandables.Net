using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

public sealed record BlockAccountCommand : Command<Account>, IApplyUnitOfWork, IApplyAggregate
{
}
