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
namespace System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides a validator that combines multiple validators and applies them to a single argument instance.
/// </summary>
/// <remarks>
/// <para>The composite validator executes each contained validator and aggregates all validation
/// results. This allows for modular validation logic by composing multiple validators.</para>
/// <para>By default, validators are executed sequentially in <see cref="Validator{TArgument}.Order"/> order.
/// Set <paramref name="enableParallelValidation"/> to <see langword="true"/> to execute validators
/// concurrently via <see cref="Task.WhenAll(IEnumerable{Task})"/>. Use parallel validation only when
/// validators are independent and thread-safe; ordering is not guaranteed in parallel mode.</para>
/// </remarks>
/// <typeparam name="TArgument">The type of the object to validate. Must be a reference type that implements <see cref="IRequiresValidation"/>.</typeparam>
/// <param name="validators">The collection of validators to apply to the argument instance. Cannot be null.</param>
/// <param name="enableParallelValidation">When <see langword="true"/>, <see cref="ValidateAsync"/> runs 
/// all validators concurrently. Defaults to <see langword="false"/> (sequential).</param>
public sealed class CompositeValidator<TArgument>(
	IEnumerable<IValidator<TArgument>> validators,
	bool enableParallelValidation = false) :
	Validator<TArgument>, ICompositeValidator<TArgument>
	where TArgument : class, IRequiresValidation
{
	private readonly IEnumerable<IValidator<TArgument>> _validators = validators
		?? throw new ArgumentNullException(nameof(validators));
	private readonly bool _enableParallelValidation = enableParallelValidation;

	/// <inheritdoc/>
	public override IReadOnlyCollection<ValidationResult> Validate(TArgument instance)
	{
		List<ValidationResult> validationResults = [];
		foreach (IValidator<TArgument> validator in _validators.OrderBy(v => v.Order))
		{
			IReadOnlyCollection<ValidationResult> results = validator.Validate(instance);
			if (results is { Count: > 0 })
			{
				validationResults.AddRange(results);
			}
		}

		return validationResults;
	}

	/// <inheritdoc/>
	public override async ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(TArgument instance)
	{
		if (_enableParallelValidation)
		{
			return await ValidateParallelAsync(instance).ConfigureAwait(false);
		}

		List<ValidationResult> validationResults = [];

		foreach (IValidator<TArgument> validator in _validators.OrderBy(v => v.Order))
		{
			IReadOnlyCollection<ValidationResult> results = await validator
				.ValidateAsync(instance)
				.ConfigureAwait(false);

			if (results is { Count: > 0 })
			{
				validationResults.AddRange(results);
			}
		}

		return validationResults;
	}

	private async ValueTask<IReadOnlyCollection<ValidationResult>> ValidateParallelAsync(TArgument instance)
	{
		Task<IReadOnlyCollection<ValidationResult>>[] tasks = [.. _validators
			.Select(v => v.ValidateAsync(instance).AsTask())];

		IReadOnlyCollection<ValidationResult>[] allResults =
			await Task.WhenAll(tasks).ConfigureAwait(false);

		List<ValidationResult> validationResults = [];
		foreach (IReadOnlyCollection<ValidationResult> results in allResults)
		{
			if (results is { Count: > 0 })
			{
				validationResults.AddRange(results);
			}
		}

		return validationResults;
	}
}
