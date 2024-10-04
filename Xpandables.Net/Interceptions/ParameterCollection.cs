
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
using System.Collections;
using System.Reflection;

namespace Xpandables.Net.Interceptions;

/// <summary>
/// An implementation of <see cref="IParameterCollection"/> 
/// that wraps a provided array
/// containing the argument values.
/// </summary>
public sealed class ParameterCollection : IParameterCollection
{
    private readonly List<Parameter> _parameters;

    /// <summary>
    /// Construct a new <see cref="ParameterCollection"/> class 
    /// that wraps the given array of arguments.
    /// </summary>
    /// <param name="methodInfo">The target method.</param>
    /// <param name="arguments">Arguments for the method, if necessary.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="methodInfo"/> is null.</exception>
    public ParameterCollection(
        MethodInfo methodInfo,
        params object?[]? arguments)
    {
        _ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
        _parameters = arguments?.Length == 0
            ? []
            : new List<Parameter>(BuildParameters(methodInfo, arguments));
    }

    /// <summary>
    /// Fetches a parameter's value by name.
    /// </summary>
    /// <param name="parameterName">The parameter name.</param>
    /// <returns>value of the named parameter.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="parameterName" /> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The 
    /// <paramref name="parameterName" /> does not exist</exception>
    public Parameter this[string parameterName]
    {
        get => _parameters[IndexForParameterName(parameterName)];
        set => _parameters[IndexForParameterName(parameterName)] = value;
    }

    /// <summary>
    /// Fetches a parameter's value by index.
    /// </summary>
    /// <param name="parameterIndex">The parameter index.</param>
    /// <returns>Value of the indexed parameter.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The 
    /// <paramref name="parameterIndex" /> does not exist</exception>
    public Parameter this[int parameterIndex]
    {
        get => _parameters[parameterIndex];
        set => _parameters[parameterIndex] = value;
    }

    /// <summary>
    /// Does this collection contain a parameter value with the given name?
    /// </summary>
    /// <param name="parameterName">Name of parameter to find.</param>
    /// <returns>True if the parameter name is 
    /// in the collection, false if not.</returns>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="parameterName"/> is null.</exception>
    public bool ContainsParameter(string parameterName)
    {
        _ = parameterName
            ?? throw new ArgumentNullException(nameof(parameterName));
        return _parameters.Exists(parameter
            => parameter.Name.Equals(
                parameterName,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns> An enumerator that can be used to 
    /// iterate through the collection.</returns>
    public IEnumerator<Parameter> GetEnumerator()
        => _parameters.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private int IndexForParameterName(string paramName)
        => _parameters
        .FindIndex(parameter => parameter.Name
        .Equals(paramName, StringComparison.OrdinalIgnoreCase)) switch
        {
            { } foundIndex when foundIndex >= 0 => foundIndex,
            _ => throw new ArgumentOutOfRangeException(
                $"Invalid parameter name : {paramName}")
        };

    private static IEnumerable<Parameter> BuildParameters(
        MethodInfo method,
        params object?[]? arguments)
    {
        foreach (var param in method
            .GetParameters()
            .Select((value, index) => new { Index = index, Value = value })
            .OrderBy(o => o.Value.Position).ToArray())
        {
            yield return Parameter
                .Build(param.Index, param.Value, arguments?[param.Index]);
        }
    }
}
