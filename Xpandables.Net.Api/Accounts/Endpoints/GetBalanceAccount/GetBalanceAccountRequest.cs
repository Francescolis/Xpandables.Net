using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetBalanceAccount;

[RequestDefinition(Path = "/accounts/balance",
    IsNullable = false,
    IsSecured = false,
    Location = RequestDefinitions.Location.Query,
    Method = RequestDefinitions.Method.GET)]
public sealed record GetBalanceAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
