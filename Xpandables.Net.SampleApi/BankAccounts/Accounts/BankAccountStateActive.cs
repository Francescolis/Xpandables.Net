using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.SampleApi.BankAccounts.Accounts;

public sealed class BankAccountStateActive : BankAccountState
{
    public override void Deposit(decimal amount, string curreny, string description)
    {
        if (amount <= 0m)
            throw new ValidationException(
                new ValidationResult(
                    "Deposit amount must be greater than zero.", [nameof(amount)]), null, amount);

        if (curreny != "EUR")
            throw new ValidationException(
                new ValidationResult(
                    "Only EUR currency is accepted for deposit.", [nameof(curreny)]), null, curreny);

        MoneyDepositEvent @event = new()
        {
            StreamId = Context.StreamId,
            StreamName = Context.StreamName,
            Amount = amount,
            Currency = curreny,
            Description = description
        };

        Context.PushEvent(@event);
    }

    public override void Withdraw(decimal amount, string curreny, string description)
    {
        if (amount <= 0m)
            throw new ValidationException(
                new ValidationResult(
                    "Withdraw amount must be greater than zero.", [nameof(amount)]), null, amount);

        if (curreny != "EUR")
            throw new ValidationException(
                new ValidationResult(
                    "Only EUR currency is accepted for withdraw.", [nameof(curreny)]), null, curreny);

        if (Balance - amount < 0m)
            throw new ValidationException(
                new ValidationResult(
                    "Insufficient funds for this withdraw.", [nameof(amount)]), null, amount);

        MoneyWithdrawEvent @event = new()
        {
            StreamId = Context.StreamId,
            StreamName = Context.StreamName,
            Amount = amount,
            Currency = curreny,
            Description = description
        };

        Context.PushEvent(@event);
    }
}
