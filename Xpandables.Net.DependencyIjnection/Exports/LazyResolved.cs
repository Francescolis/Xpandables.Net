
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
using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.DependencyInjection.Exports;

/// <summary>
/// Provides a lazy initialization of a service resolved 
/// from the <see cref="IServiceProvider"/>.
/// </summary>
/// <typeparam name="T">The type of the service to be resolved.</typeparam>
public sealed class LazyResolved<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] T>(IServiceProvider provider) :
    Lazy<T>(provider.GetRequiredService<T>())
    where T : notnull
{
}