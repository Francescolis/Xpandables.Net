using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

[MapPost("/accounts")]
public sealed record CreateAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
