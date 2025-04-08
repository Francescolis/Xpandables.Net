
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
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

using Xpandables.Net.Text;

namespace Xpandables.Net.Executions.Domains.Converters;
/// <summary>
/// Converts a primitive type to a value type and vice versa.
/// </summary>
/// <typeparam name="TPrimitive">The primitive type.</typeparam>
/// <typeparam name="TValue">The value type.</typeparam>
public sealed class PrimitiveValueConverter<TPrimitive, TValue>
    : ValueConverter<TPrimitive, TValue>
    where TValue : notnull
    where TPrimitive : struct, IPrimitive<TPrimitive, TValue>
{
    /// <summary>
    /// Constructs a new instance of 
    /// <see cref="PrimitiveValueConverter{TPrimitive, TValue}"/>.
    /// </summary>
    public PrimitiveValueConverter()
        : base(v => PrimitiveToValue(v),
            v => ValueToPrimitive(v))
    { }

    private static TPrimitive ValueToPrimitive(TValue value)
        => TPrimitive.Create(value);

    private static TValue PrimitiveToValue(TPrimitive primitive)
        => primitive.Value;
}
