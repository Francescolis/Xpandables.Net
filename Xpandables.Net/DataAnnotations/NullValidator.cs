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
using Xpandables.Net.Executions;

namespace Xpandables.Net.DataAnnotations;
/// <summary>
/// A validator that does nothing and always returns a successful result.
/// </summary>
/// <typeparam name="TArgument">The type of the argument to validate.</typeparam>
public sealed class NullValidator<TArgument> : Validator<TArgument>
    where TArgument : class, IValidationEnabled
{
    /// <inheritdoc/>
    /// Does nothing and returns an <see cref="ExecutionResult.Ok"/>.
    public override ExecutionResult Validate(TArgument instance) =>
        ExecutionResult.Ok().Build();
}
