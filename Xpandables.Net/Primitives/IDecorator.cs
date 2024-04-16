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
namespace Xpandables.Net.Primitives;

/// <summary>
/// A marker interface that allows for classes that act like decorator.
/// Usefull for the classes  not to be registered as normal implementations.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IDecorator
#pragma warning restore CA1040 // Avoid empty interfaces
{
}

/// <summary>
/// Defines a set of methods that add extension methods 
/// to the <see cref="IDecorator"/> interface.
/// </summary>
public static class IDecoratorExtensions
{
    ///<summary>
    /// Determines whether the specified type is a decorator.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <returns><see langword="true"/> if the specified type is a decorator; 
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsNotDecorator(this Type type)
        => !typeof(IDecorator).IsAssignableFrom(type);
}