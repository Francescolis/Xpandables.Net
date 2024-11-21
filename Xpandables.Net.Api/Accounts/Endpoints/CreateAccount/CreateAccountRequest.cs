using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

[HttpClient(Path = "/accounts",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Body,
    Method = HttpClientParameters.Method.POST)]
public sealed record CreateAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
