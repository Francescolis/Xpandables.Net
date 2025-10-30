﻿/*******************************************************************************
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

namespace Xpandables.Net.ExecutionResults.DataAnnotations;

/// <summary>
/// Provides a validator that performs no validation and always returns an empty set of validation results.
/// </summary>
/// <typeparam name="TArgument">The type of object to validate. Must be a reference type that implements <see cref="IRequiresValidation"/>.</typeparam>
public sealed class NullValidator<TArgument> : Validator<TArgument>
    where TArgument : class, IRequiresValidation
{
    /// <inheritdoc/>
    /// Does nothing and returns an empty collection of validation results.
    public override IReadOnlyCollection<ValidationResult> Validate(TArgument instance) => [];
}
