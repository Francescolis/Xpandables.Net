
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
namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Provides with lazy instance resolution using 
/// <see langword="AddXLazy(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>.
/// </summary>
/// <typeparam name="T">The type to be resolved.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="LazyResolved{T}" /> class that uses a preinitialized specified value from the service provider.
/// </remarks>
/// <param name="serviceProvider">The service provider used for preinitialized value.</param>
/// <exception cref="ArgumentNullException">The <paramref name="serviceProvider"/> is null.</exception>
public sealed class LazyResolved<T>(IServiceProvider serviceProvider)
    : Lazy<T>((T?)serviceProvider?.GetService(typeof(T))
        ?? throw new InvalidOperationException($"No registration found for '{typeof(T).Name}'."))
    where T : notnull
{
}
