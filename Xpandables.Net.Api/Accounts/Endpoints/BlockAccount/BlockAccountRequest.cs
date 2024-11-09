using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

[HttpClient(Path = "/accounts/block",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record BlockAccountRequest : IUseValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
