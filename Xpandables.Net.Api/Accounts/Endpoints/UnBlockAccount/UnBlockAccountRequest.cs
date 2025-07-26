using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Api.Accounts.Endpoints.UnBlockAccount;

[RestPost("/accounts/unblock")]
public sealed record UnBlockAccountRequest : IRequiresValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
