
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

using Xpandables.Net.Primitives;
using Xpandables.Net.Primitives.Converters;

namespace Xpandables.Net.Api.Persons.Primitives;

[PrimitiveJsonConverter]
public readonly record struct LastName(string Value) : IPrimitive<LastName, string>
{
    public static implicit operator string(LastName lastName)
        => lastName.Value;
    [return: NotNullIfNotNull(nameof(value))]
    public static implicit operator LastName(string? value)
        => value is null ? null : new(value);
    public static string DefaultValue => "NOLASTNAME";
    public static LastName Create(string value) => new(value);
    public static LastName Default() => new(DefaultValue);
}

[AttributeUsage(AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false,
    Inherited = true)]
public sealed class LastNameFormatAttribute :
    FormatAttribute<LastNameFormatAttribute>
{
    private const int _min = 3;
    private const int _max = byte.MaxValue;
    public override bool IsValid(object? value)
    {
        string? lastName = value as string;

        return lastName switch
        {
            null when IsNullable => true,
            null => false,
            { Length: < _min or > _max } => false,
            _ => true
        };
    }

    public override string FormatErrorMessage(string name)
        => string.Format(
            CultureInfo.CurrentCulture,
            ErrorMessageString,
            name,
            _max,
            _min);
}