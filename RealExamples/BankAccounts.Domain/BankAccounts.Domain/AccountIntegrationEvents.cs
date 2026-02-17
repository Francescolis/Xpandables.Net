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
using System.Events.Integration;

namespace BankAccounts.Domain;

public sealed record MoneyDepositIntegrationEvent : IntegrationEvent
{
	public required Guid BankAccountId { get; init; }
	public required decimal Amount { get; init; }
}

public sealed record MoneyWithdrawIntegrationEvent : IntegrationEvent
{
	public required Guid BankAccountId { get; init; }
	public required decimal Amount { get; init; }
}

public sealed record AccountBlockedIntegrationEvent : IntegrationEvent
{
	public required Guid BankAccountId { get; init; }
	public required bool IsBlocked { get; init; }
}


public sealed record AccountCreatedIntegrationEvent : IntegrationEvent
{
	public required Guid AccountId { get; init; }
	public required string AccountNumber { get; init; }
	public required string AccountType { get; init; }
	public required string Owner { get; init; }
	public required string Email { get; init; }
	public required decimal Balance { get; init; }
}
