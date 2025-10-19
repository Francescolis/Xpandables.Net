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

namespace Xpandables.Net.Api.Features.BankAccounts.Domain;

/// <summary>
/// Represents a bank account aggregate using event sourcing.
/// </summary>
public sealed class BankAccountAggregate : Aggregate, IAggregateFactory<BankAccountAggregate>
{
    private string _accountNumber = string.Empty;
    private string _owner = string.Empty;
    private string _email = string.Empty;
    private decimal _balance;
    private bool _isClosed;

    public BankAccountAggregate()
    {
        On<BankAccountCreatedEvent>(Apply);
        On<MoneyDepositedEvent>(Apply);
        On<MoneyWithdrawnEvent>(Apply);
        On<AccountClosedEvent>(Apply);
    }

    public string AccountNumber => _accountNumber;
    public string Owner => _owner;
    public string Email => _email;
    public decimal Balance => _balance;
    public bool IsClosed => _isClosed;

    public static BankAccountAggregate Create() => new();

    /// <summary>
    /// Creates a new bank account.
    /// </summary>
    public static BankAccountAggregate CreateAccount(
        Guid accountId,
        string accountNumber,
        string owner,
        string email,
        decimal initialBalance)
    {
        if (initialBalance < 0)
            throw new InvalidOperationException("Initial balance cannot be negative.");

        var aggregate = new BankAccountAggregate();
        var @event = new BankAccountCreatedEvent
        {
            StreamId = accountId,
            StreamName = nameof(BankAccountAggregate),
            AccountNumber = accountNumber,
            Owner = owner,
            Email = email,
            InitialBalance = initialBalance
        };

        aggregate.PushVersioningEvent(@event);
        return aggregate;
    }

    /// <summary>
    /// Deposits money into the account.
    /// </summary>
    public void Deposit(decimal amount, string description)
    {
        if (_isClosed)
            throw new InvalidOperationException("Cannot deposit to a closed account.");

        if (amount <= 0)
            throw new InvalidOperationException("Deposit amount must be positive.");

        var @event = new MoneyDepositedEvent
        {
            StreamId = StreamId,
            StreamName = nameof(BankAccountAggregate),
            Amount = amount,
            Description = description
        };

        PushVersioningEvent(@event);
    }

    /// <summary>
    /// Withdraws money from the account.
    /// </summary>
    public void Withdraw(decimal amount, string description)
    {
        if (_isClosed)
            throw new InvalidOperationException("Cannot withdraw from a closed account.");

        if (amount <= 0)
            throw new InvalidOperationException("Withdrawal amount must be positive.");

        if (_balance < amount)
            throw new InvalidOperationException($"Insufficient funds. Current balance: {_balance:C}");

        var @event = new MoneyWithdrawnEvent
        {
            StreamId = StreamId,
            StreamName = nameof(BankAccountAggregate),
            Amount = amount,
            Description = description
        };

        PushVersioningEvent(@event);
    }

    /// <summary>
    /// Closes the account.
    /// </summary>
    public void CloseAccount(string reason)
    {
        if (_isClosed)
            throw new InvalidOperationException("Account is already closed.");

        if (_balance > 0)
            throw new InvalidOperationException("Cannot close account with remaining balance.");

        var @event = new AccountClosedEvent
        {
            StreamId = StreamId,
            StreamName = nameof(BankAccountAggregate),
            Reason = reason
        };

        PushVersioningEvent(@event);
    }

    private void Apply(BankAccountCreatedEvent @event)
    {
        _accountNumber = @event.AccountNumber;
        _owner = @event.Owner;
        _email = @event.Email;
        _balance = @event.InitialBalance;
        _isClosed = false;
    }

    private void Apply(MoneyDepositedEvent @event)
    {
        _balance += @event.Amount;
    }

    private void Apply(MoneyWithdrawnEvent @event)
    {
        _balance -= @event.Amount;
    }

    private void Apply(AccountClosedEvent @event)
    {
        _isClosed = true;
    }

    protected override bool IsSignificantBusinessEvent(IDomainEvent domainEvent)
    {
        return domainEvent is BankAccountCreatedEvent
            or MoneyDepositedEvent
            or MoneyWithdrawnEvent
            or AccountClosedEvent;
    }
}
