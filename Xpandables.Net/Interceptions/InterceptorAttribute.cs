
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
namespace Xpandables.Net.Interceptions;

/// <summary>
/// Interceptors base attribute that allows developers to
/// apply interceptor to interfaces directly.
/// </summary>
[AttributeUsage(AttributeTargets.Interface, AllowMultiple = true)]
public abstract class InterceptorAttribute : Attribute
{
    /// <summary>
    /// Derived classes implement this method. When called, it creates a 
    /// new call handler as specified in the attribute configuration.
    /// The parameter <paramref name="serviceProvider"/> specifies the 
    /// <see cref="IServiceProvider"/> to be used when creating
    /// handlers, if necessary.
    /// Returns a new interceptor handler object.
    /// </summary>
    /// <param name="serviceProvider">The current instance of 
    /// the collection of services.</param>
    /// <returns>An implementation of <see cref="IInterceptor"/> 
    /// interface.</returns>
    public abstract IInterceptor Create(IServiceProvider serviceProvider);
}
