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
namespace Xpandables.Net.Http.RequestBuilders;
/// <summary>
/// Represents an abstract base class for building HTTP client requests.
/// </summary>
/// <typeparam name="TRequestDefinition">The type of the request definition.</typeparam>
public abstract class RequestHttpBuilder<TRequestDefinition> : IRequestHttpBuilder<TRequestDefinition>
    where TRequestDefinition : class, IRequestDefinition
{
    ///<inheritdoc/>
    public Type Type => typeof(TRequestDefinition);

    /// <summary>
    /// When overridden in a derived class, determines whether the builder
    /// instance can build the specified request implementing the
    /// specific interface type.
    /// </summary>
    /// <param name="targetType">The type of the target interface.</param>
    /// <returns><see langword="true"/> if the instance can build the
    /// specified request; otherwise, <see langword="false"/>.</returns>
    ///<inheritdoc/>
    public virtual bool CanBuild(Type targetType)
        => Type.IsAssignableFrom(targetType);

    /// <inheritdoc/>
    public abstract void Build(RequestContext context);
}
