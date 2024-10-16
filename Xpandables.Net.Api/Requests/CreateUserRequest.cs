using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;

namespace Xpandables.Net.Api.Requests;

public sealed record CreateUserRequest : IUseValidation
{
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-zA-Z0-9]*$",
        ErrorMessage = "Only letters and numbers are allowed.")]
    public required string UserName { get; init; }

    [Required]
    [EmailAddress]
    [StringLength(50, MinimumLength = 3)]
    public required string Email { get; init; }

    [Required]
    [StringLength(16, MinimumLength = 8)]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,16}$",
        ErrorMessage = "Password must contain at least one uppercase letter, " +
        "one lowercase letter, and one number.")]
    public required string Password { get; init; }
}
