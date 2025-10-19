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
using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults;
using Xpandables.Net.Tasks;
using Xpandables.Net.Validators;

namespace Xpandables.Net.Api.Features.BankAccounts.Commands;

/// <summary>
/// Command to create a new bank account.
/// </summary>
public sealed record CreateBankAccountCommand : IRequest<BankAccountResponse>
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    public required string Owner { get; init; }

    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; init; } = 0;
}

/// <summary>
/// Validator for CreateBankAccountCommand.
/// </summary>
public sealed class CreateBankAccountValidator : Validator<CreateBankAccountCommand>
{
    protected override void BuildRules()
    {
        RuleFor(x => x.Owner)
            .NotEmpty()
            .WithMessage("Owner name is required")
            .MinimumLength(3)
            .WithMessage("Owner name must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .WithMessage("Invalid email format");

        RuleFor(x => x.InitialBalance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Initial balance cannot be negative");
    }
}

/// <summary>
/// Handler for creating a new bank account.
/// </summary>
public sealed class CreateBankAccountHandler(IAggregateStore<BankAccountAggregate> aggregateStore)
    : IRequestHandler<CreateBankAccountCommand, BankAccountResponse>
{
    public async Task<ExecutionResult<BankAccountResponse>> HandleAsync(
        CreateBankAccountCommand request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accountId = Guid.NewGuid();
            var accountNumber = GenerateAccountNumber();

            var aggregate = BankAccountAggregate.CreateAccount(
                accountId,
                accountNumber,
                request.Owner,
                request.Email,
                request.InitialBalance);

            await aggregateStore.SaveAsync(aggregate, cancellationToken);

            var response = new BankAccountResponse
            {
                AccountId = accountId,
                AccountNumber = accountNumber,
                Owner = request.Owner,
                Email = request.Email,
                Balance = request.InitialBalance,
                IsClosed = false,
                Version = aggregate.StreamVersion
            };

            return ExecutionResultExtensions
                .Ok(response)
                .WithDetail($"Bank account {accountNumber} created successfully")
                .Build();
        }
        catch (Exception ex)
        {
            return ExecutionResultExtensions
                .Failure<BankAccountResponse>(System.Net.HttpStatusCode.InternalServerError)
                .WithDetail("Failed to create bank account")
                .WithError("CREATE_FAILED", ex.Message)
                .Build();
        }
    }

    private static string GenerateAccountNumber() =>
        $"ACC{DateTime.UtcNow:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
}
