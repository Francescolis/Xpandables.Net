
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
/// Specifies that the type should have its 
/// <see cref="IPrimitiveOnSerializing{TPrimitive, TValue}
/// .OnSerializing(TPrimitive)"/> 
/// method called before serialization occurs.
/// </summary>
/// <typeparam name="TPrimitive">The type of target primitive.</typeparam>
/// <typeparam name="TValue">The type of the primitive value.</typeparam>
public interface IPrimitiveOnSerializing<TPrimitive, TValue>
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
        where TValue : notnull
{
    /// <summary>
    /// The method that is called before serialization.
    /// </summary>
    /// <param name="primitive">The current primitive instance.</param>
    /// <returns>The same instance.</returns>
    TPrimitive OnSerializing(TPrimitive primitive);
}

/// <summary>
/// Specifies that the JSON type should have its 
/// <see cref="IPrimitiveOnDeserialized{TPrimitive, TValue}
/// .OnDeserialized(TPrimitive)"/> 
/// method called after deserialization occurs.
/// </summary>
/// <typeparam name="TPrimitive">The type of target primitive.</typeparam>
/// <typeparam name="TValue">The type of the primitive value.</typeparam>
public interface IPrimitiveOnDeserialized<TPrimitive, TValue>
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
    where TValue : notnull
{
    /// <summary>
    /// The method that is called after deserialization.
    /// </summary>
    /// <param name="primitive">The current primitive instance.</param>
    /// <returns>The same instance.</returns>
    TPrimitive OnDeserialized(TPrimitive primitive);
}
