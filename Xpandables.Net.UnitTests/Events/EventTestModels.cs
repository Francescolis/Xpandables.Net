/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using Xpandables.Net.Events;

namespace Xpandables.Net.UnitTests.Events;

// Domain Events
public sealed record BankAccountCreatedEvent : DomainEvent
{
    public required string AccountNumber { get; init; }
    public required string Owner { get; init; }
    public required decimal InitialBalance { get; init; }
}

public sealed record MoneyDepositedEvent : DomainEvent
{
    public required decimal Amount { get; init; }
}

public sealed record MoneyWithdrawnEvent : DomainEvent
{
    public required decimal Amount { get; init; }
}

// Aggregate
public sealed class BankAccountAggregate : Aggregate, IAggregateFactory<BankAccountAggregate>
{
    private string _accountNumber = string.Empty;
    private string _owner = string.Empty;
    private decimal _balance;

    public BankAccountAggregate()
    {
        On<BankAccountCreatedEvent>(Apply);
        On<MoneyDepositedEvent>(Apply);
        On<MoneyWithdrawnEvent>(Apply);
    }

    public string AccountNumber => _accountNumber;
    public string Owner => _owner;
    public decimal Balance => _balance;

    public static BankAccountAggregate Initialize() => new();

    public static BankAccountAggregate Create(Guid streamId, string accountNumber, string owner, decimal initialBalance)
    {
        var aggregate = new BankAccountAggregate();
        var @event = new BankAccountCreatedEvent
        {
            StreamId = streamId,
            StreamName = "BankAccount",
            AccountNumber = accountNumber,
            Owner = owner,
            InitialBalance = initialBalance
        };
        
        aggregate.PushVersioningEvent(@event);
        return aggregate;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Deposit amount must be positive");

        var @event = new MoneyDepositedEvent
        {
            StreamId = StreamId,
            StreamName = "BankAccount",
            Amount = amount
        };

        PushVersioningEvent(@event);
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be positive");
        
        if (_balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        var @event = new MoneyWithdrawnEvent
        {
            StreamId = StreamId,
            StreamName = "BankAccount",
            Amount = amount
        };

        PushVersioningEvent(@event);
    }

    private void Apply(BankAccountCreatedEvent @event)
    {
        _accountNumber = @event.AccountNumber;
        _owner = @event.Owner;
        _balance = @event.InitialBalance;
    }

    private void Apply(MoneyDepositedEvent @event)
    {
        _balance += @event.Amount;
    }

    private void Apply(MoneyWithdrawnEvent @event)
    {
        _balance -= @event.Amount;
    }

    protected override bool IsSignificantBusinessEvent(IDomainEvent domainEvent)
    {
        return domainEvent is BankAccountCreatedEvent or MoneyDepositedEvent or MoneyWithdrawnEvent;
    }
}
