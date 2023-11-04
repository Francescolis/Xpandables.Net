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
namespace Xpandables.Net.Extensions;

/// <summary>
/// Provides with methods to extend use of <see cref="object"/>.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Returns the name of the current type.
    /// </summary>
    /// <param name="obj">The target object to act on.</param>
    /// <returns>A string that represents the name of the target object.</returns>
    public static string GetTypeName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        return obj.GetType().GetNameWithoutGenericArity();
    }

    /// <summary>
    /// Returns the full name of the current type.
    /// </summary>
    /// <param name="obj">The target object to act on.</param>
    /// <returns>A string that represents the full name of the target object.</returns>
    /// <exception cref="ArgumentException">Cannot get the full name of a generic type parameter.</exception>
    public static string GetTypeFullName(this object obj)
    {
        ArgumentNullException.ThrowIfNull(obj, nameof(obj));
        if (obj.GetType().IsGenericTypeParameter)
            throw new ArgumentException(
                "Cannot get the full name of a generic type parameter.",
                obj.GetType().Name);

        return obj.GetType().AssemblyQualifiedName!;
    }
}
