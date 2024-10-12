using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Requests;

public sealed class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public override IOperationResult Validate(CreateUserRequest instance) =>
        OperationResults
            .BadRequest()
            .WithError(nameof(instance.UserName), "User name is required.")
            .WithError(nameof(instance.Email), "Email is required.")
            .WithError(nameof(instance.Password), "Password is required.")
            .Build();
}
