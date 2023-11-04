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
using System.Reflection;

namespace Xpandables.Net.Extensions;

/// <summary>
/// Provides with methods to extend use of <see cref="Type"/>.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    /// Returns the name of the type without the generic arity '`'.
    /// Useful for generic types.
    /// </summary>
    /// <param name="type">The type to act on.</param>
    /// <returns>The name of the type without the generic arity '`'.</returns>
    /// <exception cref="ArgumentNullException">The <paramref name="type"/> is null.</exception>
    public static string GetNameWithoutGenericArity(this Type type)
    {
        _ = type ?? throw new ArgumentNullException(nameof(type));

        int index = type.GetTypeInfo().Name.IndexOf('`', StringComparison.OrdinalIgnoreCase);
        return index == -1 ? type.GetTypeInfo().Name : type.GetTypeInfo().Name[..index];
    }
}
