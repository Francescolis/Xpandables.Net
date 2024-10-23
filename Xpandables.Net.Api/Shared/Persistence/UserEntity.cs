using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Api.Shared.Persistence;

public sealed class UserEntity : Entity<Guid>
{
    public string UserName { get; private set; }
    public string UserEmail { get; private set; }
    public string Password { get; private set; }
    public Collection<UserEntity> Contacts { get; private set; } = [];
    [SetsRequiredMembers]
    public UserEntity(Guid id, string userName, string userEmail, string password)
    {
        Id = id;
        UserName = userName;
        UserEmail = userEmail;
        Password = password;
    }
}
