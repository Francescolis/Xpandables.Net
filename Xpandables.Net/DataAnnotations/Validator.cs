/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Executions;
using Xpandables.Net.Operations;

namespace Xpandables.Net.DataAnnotations;
/// <summary>
/// Provides validation for instances of type <typeparamref name="TArgument"/>.
/// </summary>
/// <typeparam name="TArgument">The type of the instance to validate.</typeparam>
/// <remarks>The behavior uses <see cref="Validator.TryValidateObject(
/// object, ValidationContext, ICollection{ValidationResult}?, bool)"/>.</remarks>
public sealed class Validator<TArgument>(IServiceProvider provider) : AbstractValidator<TArgument>
    where TArgument : class, IApplyValidation
{
    private readonly IServiceProvider _provider = provider
        ?? throw new ArgumentNullException(nameof(provider));

    /// <inheritdoc/>
    public override IExecutionResult Validate(TArgument instance)
    {
        List<ValidationResult> validationResults = [];
        ValidationContext validationContext =
            new(instance, _provider, null);

        if (Validator.TryValidateObject(
            instance,
            validationContext,
            validationResults,
            true))
        {
            return ExecutionResults
                .Ok()
                .Build();
        }

        return validationResults.ToExecutionResult();
    }
}
