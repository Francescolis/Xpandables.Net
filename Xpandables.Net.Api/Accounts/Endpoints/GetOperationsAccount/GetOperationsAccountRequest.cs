using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;
using Xpandables.Net.Http.Interfaces;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

[HttpClient(Path = "/accounts/operations",
    IsNullable = false,
    IsSecured = false,
    Location = HttpClientParameters.Location.Query,
    Method = HttpClientParameters.Method.GET)]
public sealed record GetOperationsAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
