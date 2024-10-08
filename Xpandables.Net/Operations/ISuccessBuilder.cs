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
namespace Xpandables.Net.Operations;

/// <summary>
/// Interface for building a success operation result.
/// </summary>
public interface ISuccessBuilder :
    IHeaderBuilder<ISuccessBuilder>,
    ILocationBuilder<ISuccessBuilder>,
    IStatusBuilder<ISuccessBuilder>,
    IExtensionBuilder<ISuccessBuilder>,
    IClearBuilder<ISuccessBuilder>,
    IBuilder
{ }

/// <summary>
/// Interface for building a success operation result with a specific result type.
/// </summary>
/// <typeparam name="TResult">The type of the result.</typeparam>
public interface ISuccessBuilder<TResult> :
    IHeaderBuilder<ISuccessBuilder<TResult>>,
    ILocationBuilder<ISuccessBuilder<TResult>>,
    IStatusBuilder<ISuccessBuilder<TResult>>,
    IExtensionBuilder<ISuccessBuilder<TResult>>,
    IResultBuilder<ISuccessBuilder<TResult>, TResult>,
    IClearBuilder<ISuccessBuilder<TResult>>,
    IBuilder<TResult>
{ }