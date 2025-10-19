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
/// Domain event raised when a bank account is created.
/// </summary>
public sealed record BankAccountCreatedEvent : DomainEvent
{
    public required string AccountNumber { get; init; }
    public required string Owner { get; init; }
    public required string Email { get; init; }
    public required decimal InitialBalance { get; init; }
}

/// <summary>
/// Domain event raised when money is deposited into an account.
/// </summary>
public sealed record MoneyDepositedEvent : DomainEvent
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
}

/// <summary>
/// Domain event raised when money is withdrawn from an account.
/// </summary>
public sealed record MoneyWithdrawnEvent : DomainEvent
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
}

/// <summary>
/// Domain event raised when an account is closed.
/// </summary>
public sealed record AccountClosedEvent : DomainEvent
{
    public required string Reason { get; init; }
}
