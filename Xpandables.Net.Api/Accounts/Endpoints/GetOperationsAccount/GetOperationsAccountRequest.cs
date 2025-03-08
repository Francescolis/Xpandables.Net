using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

[RequestDefinition(Path = "/accounts/operations",
    IsNullable = false,
    IsSecured = false,
    Location = RequestDefinitions.Location.Query,
    Method = RequestDefinitions.Method.GET)]
public sealed record GetOperationsAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
