using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

[MapRequest(Path = "/accounts/deposit",
    IsNullable = false,
    IsSecured = false,
    Location = RequestDefinitions.Location.Body,
    Method = RequestDefinitions.Method.POST)]
public sealed record DepositAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }

    [Required]
    [Range(10, double.MaxValue)]
    public required decimal Amount { get; init; }
}
