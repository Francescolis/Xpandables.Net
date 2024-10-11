namespace Xpandables.Net.Api.Models;

public sealed class User
{
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
