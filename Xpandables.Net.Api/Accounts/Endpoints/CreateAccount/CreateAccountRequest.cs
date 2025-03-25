using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

[MapRequest(Path = "/accounts",
    IsNullable = false,
    IsSecured = false,
    Location = RequestDefinitions.Location.Body,
    Method = RequestDefinitions.Method.POST)]
public sealed record CreateAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
