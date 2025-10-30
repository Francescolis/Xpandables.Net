﻿using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.Cqrs;
using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Events;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.DepositBankAccount;

public sealed class DepositBankAccountCommand :
    IRequest<DepositBankAccountResult>, IRequiresValidation, IRequiresEventStorage
{
    [Required, FromRoute]
    public required Guid AccountId { get; init; }

    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }

    [Required]
    [StringLength(3, MinimumLength = 3)]
    public required string Currency { get; init; }

    [Required]
    [StringLength(byte.MaxValue)]
    public required string Description { get; init; }
}

public readonly record struct DepositBankAccountResult
{
    public readonly required string AccountId { get; init; }
    public readonly required string AccountNumber { get; init; }
    public readonly required decimal NewBalance { get; init; }
    public readonly required DateTime DepositedOn { get; init; }
    public readonly required string Description { get; init; }
}
