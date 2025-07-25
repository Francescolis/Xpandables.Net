using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

[RestPost("/accounts/deposit", IsSecured = false)]
public sealed record DepositAccountRequest : IRestString, IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal Amount { get; init; }
}
