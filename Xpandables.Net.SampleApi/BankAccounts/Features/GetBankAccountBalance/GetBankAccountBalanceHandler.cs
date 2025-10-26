﻿
using Xpandables.Net.Cqrs;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.SampleApi.BankAccounts.Accounts;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountBalance;

public sealed class GetBankAccountBalanceHandler(IAggregateStore aggregateStore) :
    IRequestHandler<GetBankAccountBalanceQuery, GetBankAccountBalanceResult>
{
    public async Task<ExecutionResult<GetBankAccountBalanceResult>> HandleAsync(
        GetBankAccountBalanceQuery request,
        CancellationToken cancellationToken = default)
    {
        var account = await aggregateStore.LoadAsync<BankAccount>(
            request.AccountId,
            cancellationToken).ConfigureAwait(false);

        var result = new GetBankAccountBalanceResult
        {
            AccountId = account.StreamId.ToString(),
            AccountNumber = account.CurrentState.AccountNumber,
            Balance = account.CurrentState.Balance
        };

        return ExecutionResult.Success(result);
    }
}
