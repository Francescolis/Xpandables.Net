/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Diagnostics.CodeAnalysis;

namespace Xpandables.Net.DataAnnotations;

/// <summary>
/// Provides a base implementation for validating objects that require validation using the specified argument type.
/// </summary>
/// <remarks>This class can be used as a base for custom validators that operate on objects implementing <see
/// cref="IRequiresValidation"/>. It supports both synchronous and asynchronous validation methods. The validator can be
/// constructed with an <see cref="IServiceProvider"/> to enable dependency injection during validation. Thread safety
/// is not guaranteed; if multiple threads access the same instance concurrently, external synchronization is
/// required.</remarks>
/// <typeparam name="TArgument">The type of object to validate. Must be a reference type that implements <see cref="IRequiresValidation"/>.</typeparam>
public class Validator<TArgument> : IValidator<TArgument>
    where TArgument : class, IRequiresValidation
{
    /// <summary>
    /// Contains the service provider.
    /// </summary>
    protected IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Creates a default instance of the validator.
    /// </summary>
    public Validator() { }

    /// <summary>
    /// Creates a new instance of the validator with the specified service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider to use.</param>
    public Validator(IServiceProvider serviceProvider) =>
        ServiceProvider = serviceProvider;

    /// <inheritdoc/>
    [UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code", Justification = "<Pending>")]
    public virtual IReadOnlyCollection<ValidationResult> Validate(TArgument instance)
    {
        List<ValidationResult> validationResults = [];
        ValidationContext validationContext =
            new(instance, ServiceProvider, null);

        _ = Validator.TryValidateObject(
            instance,
            validationContext,
            validationResults,
            true);

        return validationResults;
    }

    /// <inheritdoc/>
    public virtual ValueTask<IReadOnlyCollection<ValidationResult>> ValidateAsync(TArgument instance)
    {
        IReadOnlyCollection<ValidationResult> result = Validate(instance);
        return new ValueTask<IReadOnlyCollection<ValidationResult>>(result);
    }
}
