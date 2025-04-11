using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Api.Accounts.Endpoints.WithdrawAccount;

[RestPost("/accounts/withdraw")]
public sealed record WithdrawAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }

    [Required]
    [Range(10, double.MaxValue)]
    public required decimal Amount { get; init; }
}
