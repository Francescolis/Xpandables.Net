using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Executions;

namespace Xpandables.Net.Api.Accounts;

public sealed class AccountStateActive : AccountState
{
    public override ExecutionResult Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            return ExecutionResult
                .BadRequest()
                .WithError(nameof(amount), "Deposit amount must be greater than zero.")
                .Build();

        }

        DepositMade @event = new(Context, amount);

        Context.PushEvent(@event);

        return ExecutionResult.Success();
    }

    public override void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException(
                new ValidationResult(
                    "Withdraw amount must be greater than zero.",
                    [nameof(amount)]), null, amount);
        }
        if (Balance - amount < Overdraft)
        {
            throw new ValidationException(
                new ValidationResult(
                    "Insufficient funds to withdraw.",
                    [nameof(amount)]), null, amount);
        }

        WithdrawMade @event = new(Context, amount);

        Context.PushEvent(@event);
    }

    public override void Block() =>
        Context.PushEvent(new AccountBlocked(Context));

    public override void Close() =>
        Context.PushEvent(new AccountClosed(Context));
}
