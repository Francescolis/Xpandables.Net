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
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Rests;

namespace Xpandables.Net.BlazorClient.Services;

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

/// <summary>
/// Request to create a new bank account.
/// </summary>
public sealed record CreateBankAccountRequest
{
    public required string Owner { get; init; }
    public required string Email { get; init; }
    public decimal InitialBalance { get; init; }
}

/// <summary>
/// Request to deposit money.
/// </summary>
public sealed record DepositRequest
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
}

/// <summary>
/// Request to withdraw money.
/// </summary>
public sealed record WithdrawRequest
{
    public required decimal Amount { get; init; }
    public required string Description { get; init; }
}

/// <summary>
/// REST client for bank account operations.
/// </summary>
[RestClient(BaseUrl = "https://localhost:7001")]
public interface IBankAccountClient
{
    [RestPost("/api/bank-accounts")]
    Task<ExecutionResult<BankAccountResponse>> CreateAccountAsync(
        CreateBankAccountRequest request,
        CancellationToken cancellationToken = default);

    [RestGet("/api/bank-accounts/{accountId}")]
    Task<ExecutionResult<BankAccountResponse>> GetAccountAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    [RestGet("/api/bank-accounts/{accountId}/summary")]
    Task<ExecutionResult<AccountSummaryResponse>> GetAccountSummaryAsync(
        Guid accountId,
        CancellationToken cancellationToken = default);

    [RestGet("/api/bank-accounts/{accountId}/transactions")]
    Task<ExecutionResult<IAsyncEnumerable<TransactionResponse>>> GetTransactionsAsync(
        Guid accountId,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    [RestPost("/api/bank-accounts/{accountId}/deposit")]
    Task<ExecutionResult> DepositMoneyAsync(
        Guid accountId,
        DepositRequest request,
        CancellationToken cancellationToken = default);

    [RestPost("/api/bank-accounts/{accountId}/withdraw")]
    Task<ExecutionResult> WithdrawMoneyAsync(
        Guid accountId,
        WithdrawRequest request,
        CancellationToken cancellationToken = default);
}
