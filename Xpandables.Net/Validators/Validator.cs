
/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel.DataAnnotations;

using Xpandables.Net.Operations;

namespace Xpandables.Net.Validators;

/// <summary>
/// Represents a helper class that allows implementation of the <see cref="IValidator{TArgument}"/>.
/// The default behavior uses 
/// <see cref="Validator.TryValidateObject(object, ValidationContext, ICollection{ValidationResult}?, bool)"/>.
/// </summary>
/// <typeparam name="TArgument">Type of the argument.</typeparam>
/// <remarks>
/// Constructs a new instance of <see cref="Validator{TArgument}"/> with the service provider.
/// </remarks>
/// <param name="serviceProvider">The service provider to be used.</param>
public class Validator<TArgument>(IServiceProvider serviceProvider) : IValidator<TArgument>
    where TArgument : notnull
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Validates the argument and returns validation state with errors if necessary.
    /// The default behavior uses 
    /// the <see cref="Validator.TryValidateObject(object, ValidationContext, ICollection{ValidationResult}?, bool)"/>.
    /// </summary>
    /// <param name="argument">The target argument to be validated.</param>
    /// <exception cref="ArgumentNullException">The <paramref name="argument"/> is null.</exception>
    /// <exception cref="ValidationException">The exception thrown by the validator</exception>
    /// <returns>Returns a result state that contains validation information.</returns>
    public virtual IOperationResult Validate(TArgument argument)
    {
        List<ValidationResult> validationResults = [];
        ValidationContext validationContext = new(argument, _serviceProvider, null);

        if (!Validator.TryValidateObject(argument, validationContext, validationResults, true))
        {
            ElementCollection errors = [];

            foreach (ValidationResult validationResult in validationResults)
            {
                foreach (string? member in validationResult.MemberNames.Where(member => validationResult.ErrorMessage is not null))
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

    ///<inheritdoc/>
    public virtual ValueTask<IOperationResult> ValidateAsync(TArgument argument)
    {
        IOperationResult result = Validate(argument);
        return ValueTask.FromResult(result);
    }
}
