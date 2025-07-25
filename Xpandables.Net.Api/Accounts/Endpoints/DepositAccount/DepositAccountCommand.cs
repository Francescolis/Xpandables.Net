﻿using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed record DepositAccountCommand :
    DependencyRequest<Account>, IRequiresEventStorage
{
    public required decimal Amount { get; init; }
}
