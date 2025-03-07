﻿using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

public sealed record DepositAccountCommand : DeciderRequest<Account>, IApplyUnitOfWork, IApplyAggregate
{
    public required decimal Amount { get; init; }
}
