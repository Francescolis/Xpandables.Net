/*******************************************************************************
 * Copyright (C) 2025-2026 Kamersoft
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
********************************************************************************/
using System.Diagnostics.CodeAnalysis;
using System.Results;

namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides a default implementation of the Validator class for validating instances of type TArgument.
/// </summary>
/// <remarks>This class is sealed and cannot be inherited. It is designed to be used with types that require
/// validation logic, ensuring that the validation process is consistent and adheres to the specified
/// requirements.</remarks>
/// <typeparam name="TArgument">The type of the argument that requires validation. It must be a reference type that implements the
/// IRequiresValidation interface.</typeparam>
public sealed class DefaultValidator<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.AllProperties)] TArgument> : Validator<TArgument>
	where TArgument : class, IRequiresValidation
{
	/// <inheritdoc/>
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
	public override Result Validate(TArgument instance)
	{
		var validationResults = new List<ValidationResult>();
		Validator.TryValidateObject(instance, new ValidationContext(instance), validationResults, true);
		return validationResults.Count switch
		{
			> 0 => validationResults.ToResult(),
			_ => ResultWith.Success()
		};
	}
}
