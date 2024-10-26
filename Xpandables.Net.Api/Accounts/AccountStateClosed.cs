namespace Xpandables.Net.Api.Accounts;

public sealed class AccountStateClosed : AccountState
{
    public AccountStateClosed(decimal balance) => Balance = balance;
}
