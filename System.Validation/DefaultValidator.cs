using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides a default implementation of the Validator class for validating instances of type TArgument.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It is designed to be used with types that require
/// validation logic, ensuring that the validation process is consistent and adheres to the specified
/// requirements.</remarks>
/// <typeparam name="TArgument">The type of the argument that requires validation. It must be a reference type that implements the
/// IRequiresValidation interface.</typeparam>
public sealed class DefaultValidator<TArgument> : Validator<TArgument>
	where TArgument : class, IRequiresValidation
{
	/// <inheritdoc/>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
	public override IReadOnlyCollection<ValidationResult> Validate(TArgument instance)
	{
		var validationResults = new List<ValidationResult>();
		Validator.TryValidateObject(instance, new ValidationContext(instance), validationResults, true);
		return validationResults;
	}
}
