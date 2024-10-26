using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

[HttpClientRequestOptions(Path = "/accounts/balance",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Query,
    Method = HttpClientParameters.Method.GET)]
public sealed record GetBalanceAccountRequest : IUseValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
