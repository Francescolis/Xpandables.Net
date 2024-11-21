using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.DepositAccount;

[HttpClient(Path = "/accounts/deposit",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record DepositAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }

    [Required]
    [Range(10, double.MaxValue)]
    public required decimal Amount { get; init; }
}
