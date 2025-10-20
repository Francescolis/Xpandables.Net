using Xpandables.Net.Events;
using Xpandables.Net.SampleApi.EnumerationTypes;

namespace Xpandables.Net.SampleApi.BankAccounts.Accounts;

public sealed record BankAccountCreatedEvent : DomainEvent
{
    public required string Owner { get; init; }
    public required string AccountNumber { get; init; }
    public required AccountType AccountType { get; init; }
    public decimal InitialBalance { get; init; }
    public required string Email { get; init; }
}

public sealed record MoneyDepositEvent : DomainEvent
{
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Description { get; init; }
}

public sealed record MoneyWithdrawnEvent : DomainEvent
{
    public required decimal Amount { get; init; }
    public required string Currency { get; init; }
    public required string Description { get; init; }
}

public sealed record BankAccountClosedEvent : DomainEvent
{
    public required string Reason { get; init; }
}

public sealed record BankAccountBlockedEvent : DomainEvent
{
    public required DateTime BlockedOn { get; init; }
    public required string Reason { get; init; }
}

public sealed record BankAccountUnblockedEvent : DomainEvent
{
    public required DateTime ActivatedOn { get; init; }
}