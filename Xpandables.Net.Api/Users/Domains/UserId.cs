using Xpandables.Net.Text;

namespace Xpandables.Net.Api.Users.Domains;

public readonly record struct UserId(Guid Value) : IPrimitive<UserId, Guid>
{
    public static UserId Create(Guid value) => new(value);
    public static UserId Default() => new(Guid.Empty);
    public static implicit operator Guid(UserId self) => self.Value;
    public static implicit operator UserId(Guid value) => new(value);
    public static implicit operator string(UserId self) => self.Value.ToString();
}
