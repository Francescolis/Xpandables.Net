
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
namespace Xpandables.Net.Validators;

/// <summary>
/// Defines method contracts used to validate a type-specific argument by composition using a decorator.
/// The implementation must be thread-safe when working in a multi-threaded environment.
/// </summary>
/// <typeparam name="TArgument">Type of the argument to be validated.</typeparam>
public interface ICompositeValidator<in TArgument> : IValidator<TArgument>
    where TArgument : notnull
{ }