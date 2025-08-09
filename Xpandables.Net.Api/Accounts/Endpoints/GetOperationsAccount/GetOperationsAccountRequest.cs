using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Rests;

namespace Xpandables.Net.Api.Accounts.Endpoints.GetOperationsAccount;

[RestGet("/accounts/operations")]
public sealed record GetOperationsAccountRequest : IRequiresValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
