using Xpandables.Net.States;

namespace Xpandables.Net.Api.Accounts;

public abstract class AccountState : State<Account>
{
    public static readonly AccountState Active = new AccountStateActive();
    protected static readonly decimal Overdraft = -1000;
    public decimal Balance { get; internal set; }
    public virtual void Deposit(decimal amount) =>
        throw new UnauthorizedAccessException();

    public virtual void Withdraw(decimal amount) =>
        throw new UnauthorizedAccessException();

    public virtual void Block() =>
        throw new UnauthorizedAccessException();

    public virtual void UnBlock() =>
        throw new UnauthorizedAccessException();

    public virtual void Close() =>
        throw new UnauthorizedAccessException();
}
