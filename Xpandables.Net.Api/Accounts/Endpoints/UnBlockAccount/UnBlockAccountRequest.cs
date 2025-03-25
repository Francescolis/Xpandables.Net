using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

[MapRequest(Path = "/accounts/unblock",
    IsNullable = false,
    IsSecured = false,
    Location = RequestDefinitions.Location.Body,
    Method = RequestDefinitions.Method.POST)]
public sealed record UnBlockAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
