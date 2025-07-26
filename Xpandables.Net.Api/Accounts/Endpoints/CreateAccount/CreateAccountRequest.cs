using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Executions.Rests;

namespace Xpandables.Net.Api.Accounts.Endpoints.CreateAccount;

[RestPost("/accounts", IsSecured = false)]
public sealed record CreateAccountRequest : IRestString, IRequiresValidation
{
    [Required]
    public required Guid KeyId { get; init; }
}
