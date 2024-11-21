using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

[HttpClient(Path = "/accounts/balance",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Query,
    Method = HttpClientParameters.Method.GET)]
public sealed record GetBalanceAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
