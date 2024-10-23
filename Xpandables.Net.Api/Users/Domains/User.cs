using Xpandables.Net.Api.Users.Domains.Events;
using Xpandables.Net.Events.Aggregates;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Users.Domains;

public sealed class User : Aggregate
{
    private readonly List<ContactId> _contactIds = [];
    public string UserName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string Password { get; private set; } = default!;
    public IReadOnlyCollection<ContactId> ContactIds => _contactIds;

    public static User Create(UserId userId, string userName, string email, string password)
    {
        var user = new User();

        UserCreateRequested createRequested = new()
        {
            AggregateId = userId,
            UserName = userName,
            Email = email,
            Password = password
        };

        user.PushEvent(createRequested);

        return user;
    }

    public IOperationResult BeContact(ContactId contactId)
    {
        if (_contactIds.Any(x => x == contactId))
        {
            return OperationResults
                .Conflict()
                .WithError(nameof(contactId), "Contact already exists.")
                .Build();
        }

        if (contactId.Value == AggregateId)
        {
            return OperationResults
                .Conflict()
                .WithError(nameof(contactId), "Contact cannot be the same as the user.")
                .Build();
        }

        BeContactRequested @event = new(this, contactId);

        PushEvent(@event);

        return OperationResults.Ok().Build();
    }

    private User()
    {
        On<UserCreateRequested>(@event =>
        {
            AggregateId = @event.AggregateId;
            UserName = @event.UserName;
            Email = @event.Email;
            Password = @event.Password;
        });

        On<BeContactRequested>(@event => _contactIds.Add(@event.ContactId));
    }
}
