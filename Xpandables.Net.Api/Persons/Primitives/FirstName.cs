
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
public readonly record struct FirstName(string Value) : IPrimitive<FirstName, string>
{
    public static implicit operator string(FirstName FirstName)
        => FirstName.Value;
    [return: NotNullIfNotNull(nameof(value))]
    public static implicit operator FirstName(string? value)
        => value is null ? null : new(value);
    public static FirstName Create(string value) => new(value);
    public static string DefaultValue => "NO FIRSTNAME";
    public static FirstName Default() => new(DefaultValue);
}

[AttributeUsage(AttributeTargets.Property |
    AttributeTargets.Field |
    AttributeTargets.Parameter,
    AllowMultiple = false, Inherited = true)]
public sealed class FirstNameFormatAttribute :
    FormatAttribute<FirstNameFormatAttribute>
{
    private const int _min = 3;
    private const int _max = byte.MaxValue;
    public override bool IsValid(object? value)
    {
        string? firstName = value as string;

        return firstName switch
        {
            null when IsNullable => true,
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