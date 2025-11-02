using Xpandables.Net.Events.States;

namespace Xpandables.Net.SampleApi.BankAccounts.Accounts;

public abstract class BankAccountState : State<BankAccount>
{
    public string AccountNumber { get; internal set; } = string.Empty;
    public string Owner { get; internal set; } = string.Empty;
    public string Email { get; internal set; } = string.Empty;
    public decimal Balance { get; internal set; } = 0m;
    public virtual void Deposit(decimal amount, string curreny, string description) => throw new UnauthorizedAccessException();
    public virtual void Withdraw(decimal amount, string curreny, string description) => throw new UnauthorizedAccessException();
    public virtual void Block(string reason) => throw new UnauthorizedAccessException();
    public virtual void UnBlock(string reason) => throw new UnauthorizedAccessException();
    public virtual void Close(string reason) => throw new UnauthorizedAccessException();
}
