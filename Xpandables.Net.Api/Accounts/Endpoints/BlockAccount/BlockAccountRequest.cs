using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.BlockAccount;

[RequestDefinition(Path = "/accounts/block",
    IsNullable = false,
    IsSecured = false,
    Location = RequestDefinitions.Location.Body,
    Method = RequestDefinitions.Method.POST)]
public sealed record BlockAccountRequest : IApplyValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
