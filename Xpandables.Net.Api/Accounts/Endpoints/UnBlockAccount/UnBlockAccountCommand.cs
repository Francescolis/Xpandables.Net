﻿using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Tasks;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

public sealed record UnBlockAccountCommand : DeciderRequest<Account>, IApplyUnitOfWork, IApplyAggregate
{
}