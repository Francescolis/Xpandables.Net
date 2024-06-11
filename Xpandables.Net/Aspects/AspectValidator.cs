/*******************************************************************************
 * Copyright (C) 2023 Francis-Black EWANE
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

using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Aspects;

/// <summary>
/// Represents the default class implementation 
/// of the <see cref="IAspectValidator{TArgument}"/>.
/// It uses
/// <see cref="Validator.TryValidateObject(
/// object, ValidationContext, ICollection{ValidationResult}?, bool)"/>.
/// </summary>
/// <typeparam name="TArgument">Type of the argument.</typeparam>
/// <remarks>
/// Constructs a new instance of <see cref="AspectValidator{TArgument}"/> 
/// with the service provider.
/// </remarks>
/// <param name="serviceProvider">The service provider to be used.</param>
public sealed class AspectValidator<TArgument>
    (IServiceProvider serviceProvider) : IAspectValidator<TArgument>
{
    ///<inheritdoc/>
    public IOperationResult Validate(TArgument? argument)
    {
        if (argument is null)
        {
            return OperationResults
                .BadRequest()
                .WithError(nameof(argument), "The argument is null")
                .Build();
        }

        List<ValidationResult> validationResults = [];
        ValidationContext validationContext =
            new(argument, serviceProvider, null);

        if (!Validator.TryValidateObject(
            argument,
            validationContext,
            validationResults,
            true))
        {
            ElementCollection errors = [];

            foreach (ValidationResult validationResult in validationResults)
            {
                foreach (string? member in validationResult
                    .MemberNames
                    .Where(member => validationResult.ErrorMessage is not null))
                {
                    errors.Add(member, validationResult.ErrorMessage!);
                }
            }

            return OperationResults
                .BadRequest()
                .WithErrors(errors)
                .Build();
        }

        return OperationResults
            .Ok()
            .Build();
    }
}
