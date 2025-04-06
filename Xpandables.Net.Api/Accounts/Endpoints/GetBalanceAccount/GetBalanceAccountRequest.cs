using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

[MapGet("/accounts/balance")]
public sealed record GetBalanceAccountRequest : IRestRequest<int>, IRestContentQueryString, IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }

    IDictionary<string, string?>? IRestContentQueryString.GetQueryString() =>
        new Dictionary<string, string?> { [nameof(KeyId)] = KeyId.ToString() };
}
