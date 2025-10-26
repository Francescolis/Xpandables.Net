
using Xpandables.Net.Cqrs;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.SampleApi.BankAccounts.Accounts;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.DepositBankAccount;

public sealed class DepositBankAccountCommandHandler(IAggregateStore aggregateStore) :
    IRequestHandler<DepositBankAccountCommand, DepositBankAccountResult>
{
    public async Task<ExecutionResult<DepositBankAccountResult>> HandleAsync(
        DepositBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        BankAccount account = (BankAccount)await aggregateStore.LoadAsync(request.AccountId, cancellationToken);

        account.CurrentState.Deposit(request.Amount, request.Currency, request.Description);

        await aggregateStore.SaveAsync(account, cancellationToken);

        var result = new DepositBankAccountResult()
        {
            AccountId = account.StreamId.ToString(),
            NewBalance = account.CurrentState.Balance,
            AccountNumber = account.CurrentState.AccountNumber,
            DepositedOn = DateTime.UtcNow,
            Description = request.Description
        };

        return ExecutionResult.Success(result);
    }
}
