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
namespace Xpandables.Net.Api.Features.BankAccounts.Contracts;

/// <summary>
/// Response model for bank account details.
/// </summary>
public sealed record BankAccountResponse
{
    public required Guid AccountId { get; init; }
    public required string AccountNumber { get; init; }
    public required string Owner { get; init; }
    public required string Email { get; init; }
    public required decimal Balance { get; init; }
    public required bool IsClosed { get; init; }
    public required long Version { get; init; }
}

/// <summary>
/// Response model for transaction details.
/// </summary>
public sealed record TransactionResponse
{
    public required Guid TransactionId { get; init; }
    public required DateTime OccurredOn { get; init; }
    public required string Type { get; init; }
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
    public required decimal BalanceAfter { get; init; }
}

/// <summary>
/// Summary of account transactions.
/// </summary>
public sealed record AccountSummaryResponse
{
    public required BankAccountResponse Account { get; init; }
    public required int TotalTransactions { get; init; }
    public required decimal TotalDeposits { get; init; }
    public required decimal TotalWithdrawals { get; init; }
}
