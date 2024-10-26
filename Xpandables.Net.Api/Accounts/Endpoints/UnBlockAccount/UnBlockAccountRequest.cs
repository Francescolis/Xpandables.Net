using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

[HttpClientRequestOptions(Path = "/accounts/unblock",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record UnBlockAccountRequest : IUseValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
