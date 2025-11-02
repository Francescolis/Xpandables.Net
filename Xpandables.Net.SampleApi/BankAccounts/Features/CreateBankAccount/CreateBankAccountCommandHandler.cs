using Xpandables.Net.Events;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Requests;
using Xpandables.Net.SampleApi.BankAccounts.Accounts;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.CreateBankAccount;

public sealed class CreateBankAccountCommandHandler(
    IAggregateStore<BankAccount> aggregateStore) : IRequestHandler<CreateBankAccountCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        CreateBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        var accountNumber = GenerateAccountNumber();
        var accountId = Guid.NewGuid();

        var bankAccount = BankAccount.Create(
            accountId,
            accountNumber,
            request.AccountType,
            request.Owner,
            request.Email,
            request.InitialBalance);

        await aggregateStore
            .SaveAsync(bankAccount, cancellationToken)
            .ConfigureAwait(false);

        return ExecutionResult
            .Ok()
            .WithHeader("AccountId", accountId.ToString())
            .WithHeader("AccountNumber", accountNumber)
            .Build();
    }

    private static string GenerateAccountNumber() =>
        $"ACC{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
}
