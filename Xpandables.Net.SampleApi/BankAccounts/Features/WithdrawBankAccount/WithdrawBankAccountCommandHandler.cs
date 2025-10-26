
using Xpandables.Net.Cqrs;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.SampleApi.BankAccounts.Accounts;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.WithdrawBankAccount;

public sealed class WithdrawBankAccountCommandHandler(IAggregateStore aggregateStore) :
    IRequestHandler<WithdrawBankAccountCommand, WithdrawBankAccountResult>
{
    public async Task<ExecutionResult<WithdrawBankAccountResult>> HandleAsync(
        WithdrawBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        var bankAccount = await aggregateStore
            .LoadAsync<BankAccount>(request.AccountId, cancellationToken)
            .ConfigureAwait(false);

        bankAccount.CurrentState.Withdraw(request.Amount, request.Currency, request.Description);

        await aggregateStore
            .SaveAsync(bankAccount, cancellationToken)
            .ConfigureAwait(false);

        var result = new WithdrawBankAccountResult
        {
            AccountId = bankAccount.StreamId.ToString(),
            AccountNumber = bankAccount.CurrentState.AccountNumber,
            NewBalance = bankAccount.CurrentState.Balance,
            WithdrawnOn = DateTime.UtcNow,
            Description = request.Description
        };

        return ExecutionResult.Success(result);
    }
}
