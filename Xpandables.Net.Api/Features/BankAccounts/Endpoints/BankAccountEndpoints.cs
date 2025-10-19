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
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.Api.Features.BankAccounts.Commands;
using Xpandables.Net.Api.Features.BankAccounts.Contracts;
using Xpandables.Net.Api.Features.BankAccounts.Queries;
using Xpandables.Net.AspNetCore;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;

namespace Xpandables.Net.Api.Features.BankAccounts.Endpoints;

/// <summary>
/// Registers bank account endpoints using minimal API.
/// </summary>
public sealed class BankAccountEndpoints : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/bank-accounts")
            .WithTags("Bank Accounts")
            .WithOpenApi();

        // POST /api/bank-accounts - Create account
        group.MapPost("", CreateAccount)
            .WithName("CreateBankAccount")
            .WithSummary("Creates a new bank account")
            .Produces<BankAccountResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        // GET /api/bank-accounts/{id} - Get account
        group.MapGet("{id:guid}", GetAccount)
            .WithName("GetBankAccount")
            .WithSummary("Gets bank account details")
            .Produces<BankAccountResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/bank-accounts/{id}/summary - Get account summary
        group.MapGet("{id:guid}/summary", GetAccountSummary)
            .WithName("GetAccountSummary")
            .WithSummary("Gets bank account summary with transaction statistics")
            .Produces<AccountSummaryResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // GET /api/bank-accounts/{id}/transactions - Get transactions (streamed)
        group.MapGet("{id:guid}/transactions", GetTransactions)
            .WithName("GetAccountTransactions")
            .WithSummary("Gets all account transactions with pagination support")
            .Produces<IAsyncEnumerable<TransactionResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        // POST /api/bank-accounts/{id}/deposit - Deposit money
        group.MapPost("{id:guid}/deposit", DepositMoney)
            .WithName("DepositMoney")
            .WithSummary("Deposits money into an account")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        // POST /api/bank-accounts/{id}/withdraw - Withdraw money
        group.MapPost("{id:guid}/withdraw", WithdrawMoney)
            .WithName("WithdrawMoney")
            .WithSummary("Withdraws money from an account")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> CreateAccount(
        [FromBody] CreateBankAccountCommand command,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        ExecutionResult<BankAccountResponse> result = await mediator.SendAsync(command, cancellationToken);
        return result.ToResult();
    }

    private static async Task<IResult> GetAccount(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetBankAccountQuery { AccountId = id };
        ExecutionResult<BankAccountResponse> result = await mediator.SendAsync(query, cancellationToken);
        return result.ToResult();
    }

    private static async Task<IResult> GetAccountSummary(
        [FromRoute] Guid id,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountSummaryQuery { AccountId = id };
        ExecutionResult<AccountSummaryResponse> result = await mediator.SendAsync(query, cancellationToken);
        return result.ToResult();
    }

    private static async Task<IResult> GetTransactions(
        [FromRoute] Guid id,
        [FromQuery] int pageSize,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetAccountTransactionsQuery
        {
            AccountId = id,
            PageSize = pageSize > 0 ? pageSize : 20
        };

        var result = await mediator.SendAsync(query, cancellationToken);
        return result.ToAsyncPagedResult();
    }

    private static async Task<IResult> DepositMoney(
        [FromRoute] Guid id,
        [FromBody] DepositRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new DepositMoneyCommand
        {
            AccountId = id,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await mediator.SendAsync(command, cancellationToken);
        return result.ToResult();
    }

    private static async Task<IResult> WithdrawMoney(
        [FromRoute] Guid id,
        [FromBody] WithdrawRequest request,
        [FromServices] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var command = new WithdrawMoneyCommand
        {
            AccountId = id,
            Amount = request.Amount,
            Description = request.Description
        };

        var result = await mediator.SendAsync(command, cancellationToken);
        return result.ToResult();
    }

    private sealed record DepositRequest
    {
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
    }

    private sealed record WithdrawRequest
    {
        public required decimal Amount { get; init; }
        public required string Description { get; init; }
    }
}
