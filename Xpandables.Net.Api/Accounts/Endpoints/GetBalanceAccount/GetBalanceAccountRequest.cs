using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

[MapGet("/accounts/balance")]
public sealed record GetBalanceAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
