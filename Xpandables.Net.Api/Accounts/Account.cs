using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;
using Xpandables.Net.Executions.Domains;

namespace Xpandables.Net.Api.Accounts;

public sealed class Account : AggregateState<Account, AccountState>
{
    public static Account Create(Guid keyId)
    {
        Account account = new();
        account.PushEvent(new AccountCreated { AggregateId = keyId });
        return account;
    }

    public ExecutionResult Deposit(decimal amount) => CurrentState.Deposit(amount);

    public void Withdraw(decimal amount) => CurrentState.Withdraw(amount);

    public void Block() => CurrentState.Block();

    public void UnBlock() => CurrentState.UnBlock();

    public void Close() => CurrentState.Close();

    public Account() : base(new AccountStateActive())
    {
        On<AccountCreated>(@event => KeyId = @event.AggregateId);

        On<AccountBlocked>(@event =>
            TransitionToState(new AccountStateBlocked(CurrentState.Balance)));

        On<AccountClosed>(@event =>
            TransitionToState(new AccountStateClosed(CurrentState.Balance)));
        On<AccountUnBlocked>(@event =>
            TransitionToState(new AccountStateActive() { Balance = CurrentState.Balance }));

        On<DepositMade>(@event => CurrentState.Balance += @event.Amount);

        On<WithdrawMade>(@event => CurrentState.Balance -= @event.Amount);
    }
}
