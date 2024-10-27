﻿using Xpandables.Net.Api.Accounts.Events;
using Xpandables.Net.Events.Aggregates;

namespace Xpandables.Net.Api.Accounts;

public sealed class Account : AggregateStateContext<Account, AccountState>
{
    public static Account Create(Guid keyId)
    {
        Account account = new();
        account.PushEvent(new AccountCreated { AggregateId = keyId });
        return account;
    }

    public void Deposit(decimal amount) => CurrentState.Deposit(amount);

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

        On<DepositMade>(@event => CurrentState.Balance += @event.Amount);

        On<WithdrawMade>(@event => CurrentState.Balance -= @event.Amount);
    }
}