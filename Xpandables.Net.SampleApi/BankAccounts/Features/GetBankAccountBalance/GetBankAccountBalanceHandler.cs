using Microsoft.EntityFrameworkCore;

using Xpandables.Net.SampleApi.ReadStorage;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountBalance;

public sealed class GetBankAccountBalanceHandler(BankAccountDataContext context) :
    IRequestHandler<GetBankAccountBalanceQuery, GetBankAccountBalanceResult>
{
    public async Task<OperationResult<GetBankAccountBalanceResult>> HandleAsync(
        GetBankAccountBalanceQuery request,
        CancellationToken cancellationToken = default)
    {
        GetBankAccountBalanceResult? account = await context.BankAccounts
            .AsNoTracking()
            .Where(a => a.KeyId == request.AccountId)
            .Select(a => new GetBankAccountBalanceResult
            {
                AccountId = a.KeyId.ToString(),
                AccountNumber = a.AccountNumber,
                Balance = a.Balance
            })
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return account.HasValue
            ? OperationResult.Success(account.Value)
            : OperationResult.NotFound(nameof(request.AccountId), "Account not found");
    }
}
