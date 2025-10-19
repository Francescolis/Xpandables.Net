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
using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Api.Features.BankAccounts.Contracts;
using Xpandables.Net.Api.Features.BankAccounts.Domain;
using Xpandables.Net.Async;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Features.BankAccounts.Queries;

/// <summary>
/// Query to get bank account details.
/// </summary>
public sealed record GetBankAccountQuery : IRequest<BankAccountResponse>
{
    public required Guid AccountId { get; init; }
}

/// <summary>
/// Handler for getting bank account details.
/// </summary>
public sealed class GetBankAccountHandler(IAggregateStore<BankAccountAggregate> aggregateStore)
    : IRequestHandler<GetBankAccountQuery, BankAccountResponse>
{
    public async Task<ExecutionResult<BankAccountResponse>> HandleAsync(
        GetBankAccountQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await aggregateStore.LoadAsync(request.AccountId, cancellationToken);

            var response = new BankAccountResponse
            {
                AccountId = aggregate.StreamId,
                AccountNumber = aggregate.AccountNumber,
                Owner = aggregate.Owner,
                Email = aggregate.Email,
                Balance = aggregate.Balance,
                IsClosed = aggregate.IsClosed,
                Version = aggregate.StreamVersion
            };

            return ExecutionResultExtensions.Ok(response).Build();
        }
        catch (ValidationException)
        {
            return ExecutionResultExtensions
                .Failure<BankAccountResponse>(System.Net.HttpStatusCode.NotFound)
                .WithDetail("Account not found")
                .WithError("NOT_FOUND", $"No account found with ID {request.AccountId}")
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResultExtensions
                .Failure<BankAccountResponse>(System.Net.HttpStatusCode.InternalServerError)
                .WithDetail("Failed to retrieve account")
                .WithError("QUERY_FAILED", ex.Message)
                .Build();
        }
    }
}

/// <summary>
/// Query to get account transactions with paging support.
/// </summary>
public sealed record GetAccountTransactionsQuery : IStreamRequest<TransactionResponse>
{
    public required Guid AccountId { get; init; }
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Handler for getting account transactions.
/// </summary>
public sealed class GetAccountTransactionsHandler(IEventStore eventStore)
    : IStreamRequestHandler<GetAccountTransactionsQuery, TransactionResponse>
{
    public async Task<ExecutionResult<IAsyncPagedEnumerable<TransactionResponse>>> HandleAsync(
        GetAccountTransactionsQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var readRequest = new ReadStreamRequest
            {
                StreamId = request.AccountId,
                FromVersion = -1,
                MaxCount = 0
            };

            async IAsyncEnumerable<TransactionResponse> GetTransactions()
            {
                decimal runningBalance = 0;

                await foreach (var envelope in eventStore.ReadStreamAsync(
                    readRequest,
                    cancellationToken: cancellationToken))
                {
                    if (envelope.Event is BankAccountCreatedEvent created)
                    {
                        runningBalance = created.InitialBalance;
                        yield return new TransactionResponse
                        {
                            TransactionId = envelope.EventId,
                            OccurredOn = envelope.OccurredOn,
                            Type = "Account Created",
                            Amount = created.InitialBalance,
                            Description = "Initial deposit",
                            BalanceAfter = runningBalance
                        };
                    }
                    else if (envelope.Event is MoneyDepositedEvent deposit)
                    {
                        runningBalance += deposit.Amount;
                        yield return new TransactionResponse
                        {
                            TransactionId = envelope.EventId,
                            OccurredOn = envelope.OccurredOn,
                            Type = "Deposit",
                            Amount = deposit.Amount,
                            Description = deposit.Description,
                            BalanceAfter = runningBalance
                        };
                    }
                    else if (envelope.Event is MoneyWithdrawnEvent withdrawal)
                    {
                        runningBalance -= withdrawal.Amount;
                        yield return new TransactionResponse
                        {
                            TransactionId = envelope.EventId,
                            OccurredOn = envelope.OccurredOn,
                            Type = "Withdrawal",
                            Amount = withdrawal.Amount,
                            Description = withdrawal.Description,
                            BalanceAfter = runningBalance
                        };
                    }
                }
            }

            var totalCount = 0;
            var transactions = GetTransactions();

            var pagedResult = new AsyncPagedEnumerable<TransactionResponse>(
                transactions,
                ct => new ValueTask<Pagination>(Pagination.Create(
                    request.PageSize,
                    1,
                    null,
                    totalCount)));

            return ExecutionResultExtensions
                .Ok<IAsyncPagedEnumerable<TransactionResponse>>(pagedResult)
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResultExtensions
                .Failure<IAsyncPagedEnumerable<TransactionResponse>>(
                    System.Net.HttpStatusCode.InternalServerError)
                .WithDetail("Failed to retrieve transactions")
                .WithError("QUERY_FAILED", ex.Message)
                .Build();
        }
    }
}

/// <summary>
/// Query to get account summary.
/// </summary>
public sealed record GetAccountSummaryQuery : IRequest<AccountSummaryResponse>
{
    public required Guid AccountId { get; init; }
}

/// <summary>
/// Handler for getting account summary.
/// </summary>
public sealed class GetAccountSummaryHandler(
    IAggregateStore<BankAccountAggregate> aggregateStore,
    IEventStore eventStore)
    : IRequestHandler<GetAccountSummaryQuery, AccountSummaryResponse>
{
    public async Task<ExecutionResult<AccountSummaryResponse>> HandleAsync(
        GetAccountSummaryQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await aggregateStore.LoadAsync(request.AccountId, cancellationToken);

            var readRequest = new ReadStreamRequest
            {
                StreamId = request.AccountId,
                FromVersion = -1,
                MaxCount = 0
            };

            int totalTransactions = 0;
            decimal totalDeposits = 0;
            decimal totalWithdrawals = 0;

            await foreach (var envelope in eventStore.ReadStreamAsync(
                readRequest,
                cancellationToken: cancellationToken))
            {
                if (envelope.Event is MoneyDepositedEvent deposit)
                {
                    totalTransactions++;
                    totalDeposits += deposit.Amount;
                }
                else if (envelope.Event is MoneyWithdrawnEvent withdrawal)
                {
                    totalTransactions++;
                    totalWithdrawals += withdrawal.Amount;
                }
            }

            var response = new AccountSummaryResponse
            {
                Account = new BankAccountResponse
                {
                    AccountId = aggregate.StreamId,
                    AccountNumber = aggregate.AccountNumber,
                    Owner = aggregate.Owner,
                    Email = aggregate.Email,
                    Balance = aggregate.Balance,
                    IsClosed = aggregate.IsClosed,
                    Version = aggregate.StreamVersion
                },
                TotalTransactions = totalTransactions,
                TotalDeposits = totalDeposits,
                TotalWithdrawals = totalWithdrawals
            };

            return ExecutionResultExtensions.Ok(response).Build();
        }
        catch (ValidationException)
        {
            return ExecutionResultExtensions
                .Failure<AccountSummaryResponse>(System.Net.HttpStatusCode.NotFound)
                .WithDetail("Account not found")
                .WithError("NOT_FOUND", $"No account found with ID {request.AccountId}")
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResultExtensions
                .Failure<AccountSummaryResponse>(System.Net.HttpStatusCode.InternalServerError)
                .WithDetail("Failed to retrieve account summary")
                .WithError("QUERY_FAILED", ex.Message)
                .Build();
        }
    }
}
