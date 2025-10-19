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

using Xpandables.Net.Api.Features.BankAccounts.Domain;
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;
using Xpandables.Net.Validators;

namespace Xpandables.Net.Api.Features.BankAccounts.Commands;

/// <summary>
/// Command to deposit money into an account.
/// </summary>
public sealed record DepositMoneyCommand : IRequest
{
    [Required]
    public required Guid AccountId { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }

    [Required]
    [StringLength(200)]
    public required string Description { get; init; }
}

/// <summary>
/// Validator for DepositMoneyCommand.
/// </summary>
public sealed class DepositMoneyValidator : Validator<DepositMoneyCommand>
{
    protected override void BuildRules()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Deposit amount must be greater than zero");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");
    }
}

/// <summary>
/// Handler for depositing money.
/// </summary>
public sealed class DepositMoneyHandler(IAggregateStore<BankAccountAggregate> aggregateStore)
    : IRequestHandler<DepositMoneyCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        DepositMoneyCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await aggregateStore.LoadAsync(request.AccountId, cancellationToken);

            aggregate.Deposit(request.Amount, request.Description);

            await aggregateStore.SaveAsync(aggregate, cancellationToken);

            return ExecutionResultExtensions
                .Ok()
                .WithDetail($"Deposited {request.Amount:C} successfully. New balance: {aggregate.Balance:C}")
                .Build();
        }
        catch (ValidationException ex)
        {
            return ExecutionResultExtensions
                .Failure(System.Net.HttpStatusCode.NotFound)
                .WithDetail("Account not found")
                .WithError("NOT_FOUND", ex.Message)
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResultExtensions
                .Failure(System.Net.HttpStatusCode.BadRequest)
                .WithDetail("Invalid operation")
                .WithError("INVALID_OPERATION", ex.Message)
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResultExtensions
                .Failure(System.Net.HttpStatusCode.InternalServerError)
                .WithDetail("Failed to deposit money")
                .WithError("DEPOSIT_FAILED", ex.Message)
                .Build();
        }
    }
}

/// <summary>
/// Command to withdraw money from an account.
/// </summary>
public sealed record WithdrawMoneyCommand : IRequest
{
    [Required]
    public required Guid AccountId { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }

    [Required]
    [StringLength(200)]
    public required string Description { get; init; }
}

/// <summary>
/// Validator for WithdrawMoneyCommand.
/// </summary>
public sealed class WithdrawMoneyValidator : Validator<WithdrawMoneyCommand>
{
    protected override void BuildRules()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("Account ID is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Withdrawal amount must be greater than zero");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(200)
            .WithMessage("Description cannot exceed 200 characters");
    }
}

/// <summary>
/// Handler for withdrawing money.
/// </summary>
public sealed class WithdrawMoneyHandler(IAggregateStore<BankAccountAggregate> aggregateStore)
    : IRequestHandler<WithdrawMoneyCommand>
{
    public async Task<ExecutionResult> HandleAsync(
        WithdrawMoneyCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var aggregate = await aggregateStore.LoadAsync(request.AccountId, cancellationToken);

            aggregate.Withdraw(request.Amount, request.Description);

            await aggregateStore.SaveAsync(aggregate, cancellationToken);

            return ExecutionResultExtensions
                .Ok()
                .WithDetail($"Withdrew {request.Amount:C} successfully. New balance: {aggregate.Balance:C}")
                .Build();
        }
        catch (ValidationException ex)
        {
            return ExecutionResultExtensions
                .Failure(System.Net.HttpStatusCode.NotFound)
                .WithDetail("Account not found")
                .WithError("NOT_FOUND", ex.Message)
                .Build();
        }
        catch (InvalidOperationException ex)
        {
            return ExecutionResultExtensions
                .Failure(System.Net.HttpStatusCode.BadRequest)
                .WithDetail("Invalid operation")
                .WithError("INVALID_OPERATION", ex.Message)
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResultExtensions
                .Failure(System.Net.HttpStatusCode.InternalServerError)
                .WithDetail("Failed to withdraw money")
                .WithError("WITHDRAWAL_FAILED", ex.Message)
                .Build();
        }
    }
}
