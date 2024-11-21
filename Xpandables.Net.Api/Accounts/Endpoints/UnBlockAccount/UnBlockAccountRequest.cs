using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

[HttpClient(Path = "/accounts/unblock",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record UnBlockAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
