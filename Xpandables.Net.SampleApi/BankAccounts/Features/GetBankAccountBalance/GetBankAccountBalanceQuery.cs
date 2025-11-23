using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Mvc;

namespace Xpandables.Net.SampleApi.BankAccounts.Features.GetBankAccountBalance;

public sealed class GetBankAccountBalanceQuery : IRequest<GetBankAccountBalanceResult>
{
    [Required, FromRoute]
    public required Guid AccountId { get; init; }
}

public readonly record struct GetBankAccountBalanceResult
{
    public readonly required string AccountId { get; init; }
    public readonly required string AccountNumber { get; init; }
    public readonly required decimal Balance { get; init; }
}