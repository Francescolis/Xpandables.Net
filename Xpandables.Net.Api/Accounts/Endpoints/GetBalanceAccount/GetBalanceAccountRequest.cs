using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

[RestGet("/accounts/balance")]
public sealed record GetBalanceAccountRequest : IRestQueryString, IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }

    IDictionary<string, string?>? IRestQueryString.GetQueryString() =>
        new Dictionary<string, string?> { [nameof(KeyId)] = KeyId.ToString() };
}
