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

namespace Xpandables.Net.Http.Builders.Requests;

/// <summary>
/// An abstract class for building REST requests, requiring a specific content type that implements IRestContent.
/// </summary>
/// <typeparam name="TRestContent">Specifies the type of content that the REST request will handle, 
/// ensuring it adheres to the IRestContent interface.</typeparam>
public abstract class RestRequestBuilder<TRestContent> : IRestRequestBuilder<TRestContent>
    where TRestContent : class, IRestContent
{
    ///<inheritdoc/>
    public Type Type => typeof(TRestContent);

    /// <inheritdoc/>
    public abstract void Build(RestRequestContext context);

    /// <inheritdoc/>
    public virtual bool CanBuild(Type targetType) => Type.IsAssignableFrom(targetType);
}
