using Xpandables.Net.Api.Accounts.Events;

namespace Xpandables.Net.Api.Accounts;

public sealed class AccountStateActive : AccountState
{
    public override void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException(
                "Deposit amount must be greater than zero.");
        }

        DepositMade @event = new(Context, amount) { Amount = amount };

        Context.PushEvent(@event);
    }

    public override void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new InvalidOperationException(
                "Withdraw amount must be greater than zero.");
        }
        if (Balance - amount < Overdraft)
        {
            throw new InvalidOperationException(
                "Insufficient funds to withdraw.");
        }

        WithdrawMade @event = new(Context, amount) { Amount = amount };

        Context.PushEvent(@event);
    }

    public override void Block() =>
        Context.PushEvent(new AccountBlocked(Context));

    public override void Close() =>
        Context.PushEvent(new AccountClosed(Context));
}
