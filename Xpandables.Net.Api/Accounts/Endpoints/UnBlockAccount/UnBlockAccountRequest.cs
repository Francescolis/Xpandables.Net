using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

[MapPost("/accounts/unblock")]
public sealed record UnBlockAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
