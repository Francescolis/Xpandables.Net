using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Http;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

[RestGet("/accounts/operations")]
public sealed record GetOperationsAccountRequest : IValidationEnabled
{
    [Required]
    public required Guid KeyId { get; init; }
}
