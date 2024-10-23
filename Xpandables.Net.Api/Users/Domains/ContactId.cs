using Xpandables.Net.Text;

namespace Xpandables.Net.Api.Users.Domains;

public readonly record struct ContactId(Guid Value) : IPrimitive<ContactId, Guid>
{
    public static ContactId Create(Guid value) => new(value);
    public static ContactId Default() => new(Guid.Empty);

    public static implicit operator Guid(ContactId self) => self.Value;
    public static implicit operator ContactId(Guid value) => new(value);
    public static implicit operator string(ContactId self) => self.Value.ToString();
}
