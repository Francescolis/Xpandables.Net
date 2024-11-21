using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

[HttpClient(Path = "/accounts/block",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record BlockAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
