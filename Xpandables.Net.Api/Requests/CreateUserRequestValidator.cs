using System.ComponentModel.DataAnnotations;

using Xpandables.Net.DataAnnotations;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Requests;

public sealed class CreateUserRequestValidator<T> : AbstractValidator<T>
    where T : class, IUseValidation
{
    public override IOperationResult Validate(T instance)
    {
        if (instance is not CreateUserRequest createUserRequest)
        {
            return OperationResults
                .BadRequest()
                .WithError("Request", "Invalid request type.")
                .Build();
        }

        List<ValidationResult> validationResults = [];
        ValidationContext validationContext =
            new(createUserRequest, null, null);

        if (Validator.TryValidateObject(
            createUserRequest,
            validationContext,
            validationResults,
            true))
        {
            return OperationResults
                .Ok()
                .Build();
        }

        return validationResults.ToOperationResult();
    }
}
