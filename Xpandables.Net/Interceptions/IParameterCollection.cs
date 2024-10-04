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
using System.Reflection;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// This interface represents a list of either input or output
/// parameters. It implements a fixed size list.
/// </summary>
public interface IParameterCollection : IEnumerable<Parameter>
{
    /// <summary>
    /// Fetches a parameter's value by name.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>value of the named parameter.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="parameterName"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The 
    /// <paramref name="parameterName"/> does not exist</exception>
    Parameter this[string parameterName] { get; set; }

    /// <summary>
    /// Fetches a parameter's value by index.
    /// </summary>
    /// <param name="parameterIndex">The parameter index.</param>
    /// <returns>Value of the indexed parameter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The 
    /// <paramref name="parameterIndex"/> does not exist</exception>
    Parameter this[int parameterIndex] { get; set; }

    /// <summary>
    /// Does this collection contain a parameter value with the given name?
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>True if the parameter name is in the collection, 
    /// false if not.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="parameterName"/> is null.</exception>
    bool ContainsParameter(string parameterName);
}


/// <summary>
/// Defines the structure of a argument of a method at runtime.
/// </summary>
public sealed record class Parameter
{
    /// <summary>
    /// Builds a new instance of <see cref="Parameter"/> with 
    /// the position, name and value.
    /// </summary>
    /// <param name="position">The parameter position in the method signature</param>
    /// <param name="source">The parameter info to act on.</param>
    /// <param name="value">The value of the parameter.</param>
    /// <returns>An instance of new <see cref="Parameter"/>.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="source"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The <paramref name="position"/> must be greater
    /// or equal to zero.</exception>
    public static Parameter Build(
        int position,
        ParameterInfo source,
        object? value)
    {
        ArgumentNullException.ThrowIfNull(source);

        return new Parameter(
            position,
            source.Name!,
            value,
            GetTypeFromParameterInfo(source),
            GetPassedStatusFromParameterInfo(source));
    }

    private Parameter(
        int position,
        string name,
        object? value,
        Type type,
        PassingState isPassed)
    {
        if (position < 0)
        {
            throw new ArgumentOutOfRangeException(
                $"{position} must be greater or equal to zero.");
        }

        Position = position;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value;
        Type = type ?? throw new ArgumentNullException(nameof(type));
        PassingBy = isPassed;
    }

    /// <summary>
    /// Gets the index position of the parameter in the method signature.
    /// The value must be greater or equal to zero, otherwise the interface 
    /// contract will throw an <see cref="ArgumentOutOfRangeException"/>.
    /// </summary>
    public int Position { get; }

    /// <summary>
    /// Gets the name of the parameter as defined in the method signature.
    /// The value can not be null, otherwise the interface 
    /// contract will throw an <see cref="ArgumentNullException"/>.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value of the parameter at runtime.
    /// </summary>
    public object? Value { get; private set; }

    /// <summary>
    /// Gets the type of the argument.
    /// </summary>
    public Type Type { get; }

    /// <summary>
    /// Determines whether the argument is <see langword="out"/>, 
    /// <see langword="in"/>
    /// or by <see langword="ref"/> parameter.
    /// </summary>
    public PassingState PassingBy { get; }

    /// <summary>
    /// Sets a new value to the parameter.
    /// The new value type must match the argument <see cref="Type"/>,
    /// otherwise it will throw a <see cref="FormatException"/>
    /// </summary>
    /// <param name="newValue">The new value to be used.</param>
    public Parameter ChangeValueTo(object? newValue)
    {
        Value = newValue;
        return this;
    }

    /// <summary>
    /// Determines whether the argument is <see langword="out"/>, 
    /// <see langword="in"/>
    /// or <see langword="ref"/> parameter.
    /// </summary>
    [Serializable]
    public enum PassingState
    {
        /// <summary>
        /// Standard parameter.
        /// </summary>
        In = 0,

        /// <summary>
        /// <see langword="out"/> parameter.
        /// </summary>
        Out = 1,

        /// <summary>
        /// <see langword="ref"/> parameter.
        /// </summary>
        Ref = 2
    }

    /// <summary>
    /// Returns the <see cref="PassingState"/> of the parameter.
    /// </summary>
    /// <param name="parameterInfo">The parameter to act on.</param>
    /// <returns>A <see cref="PassingState"/> that matches the parameter
    /// .</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="parameterInfo"/> is null.</exception>
    private static PassingState GetPassedStatusFromParameterInfo(
        ParameterInfo parameterInfo)
    {
        ArgumentNullException.ThrowIfNull(parameterInfo);

        if (parameterInfo.IsOut)
        {
            return PassingState.Out;
        }

        return parameterInfo.ParameterType.IsByRef
                ? PassingState.Ref
                : PassingState.In;
    }

    /// <summary>
    /// Returns the type of the parameter.
    /// </summary>
    /// <param name="parameterInfo">The parameter to act on.</param>
    /// <returns>The parameter type.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="parameterInfo"/> is null.</exception>
    private static Type GetTypeFromParameterInfo(ParameterInfo parameterInfo)
    {
        ArgumentNullException.ThrowIfNull(parameterInfo);

        return parameterInfo.ParameterType.IsByRef
                   ? parameterInfo.ParameterType.GetElementType()!
                   : parameterInfo.ParameterType;
    }
}