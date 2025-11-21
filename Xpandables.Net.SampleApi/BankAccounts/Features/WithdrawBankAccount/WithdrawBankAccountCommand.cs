using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.EventSourcing;
using Xpandables.Net.Requests;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.WithdrawBankAccount;

public sealed record WithdrawBankAccountCommand :
    IRequest<WithdrawBankAccountResult>, IRequiresValidation, IRequiresEventStorage
{
    internal Guid AccountId { get; init; }
    [Required]
    [Range(0.01, double.MaxValue)]
    public required decimal Amount { get; init; }
    [Required]
    [StringLength(3, MinimumLength = 3)]
    public required string Currency { get; init; }
    [Required]
    [StringLength(250, MinimumLength = 3)]
    public required string Description { get; init; }
}

public readonly record struct WithdrawBankAccountResult
{
    public required string AccountId { get; init; }
    public required string AccountNumber { get; init; }
    public required decimal NewBalance { get; init; }
    public required DateTime WithdrawnOn { get; init; }
    public required string Description { get; init; }
}
