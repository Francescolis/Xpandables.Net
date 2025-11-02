using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Events;
using Xpandables.Net.ExecutionResults.DataAnnotations;
using Xpandables.Net.Requests;
using Xpandables.Net.SampleApi.EnumerationTypes;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.CreateBankAccount;

public sealed class CreateBankAccountCommand :
    IRequest<CreateBankAccountResult>, IRequiresValidation, IRequiresEventStorage
{
    [Required, StringLength(byte.MaxValue, MinimumLength = 3)]
    public required string Owner { get; init; }

    [Required]
    public required AccountType AccountType { get; init; }

    [Required, EmailAddress]
    public required string Email { get; init; }

    [Range(0, double.MaxValue)]
    public decimal InitialBalance { get; init; }
}

public readonly record struct CreateBankAccountResult
{
    public readonly required string AccountId { get; init; }
    public readonly required string AccountNumber { get; init; }
}