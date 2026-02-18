/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Events.Domain;

namespace BankAccounts.Domain;

public sealed record AccountCreatedEvent : DomainEvent
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

public sealed record MoneyWithdrawEvent : DomainEvent
{
	public required decimal Amount { get; init; }
	public required string Currency { get; init; }
	public required string Description { get; init; }
}

public sealed record AccountClosedEvent : DomainEvent
{
	public required DateTime ClosedOn { get; init; }
	public required string Reason { get; init; }
}

public sealed record AccountBlockedEvent : DomainEvent
{
	public required DateTime BlockedOn { get; init; }
	public required string Reason { get; init; }
}

public sealed record AccountUnblockedEvent : DomainEvent
{
	public required DateTime UnBlockedOn { get; init; }
	public required string Reason { get; init; }
}
