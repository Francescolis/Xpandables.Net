/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
using System.Text.RegularExpressions;

namespace Xpandables.Net.Events.Internals;

internal static partial class TypeNameFormater
{
    public static string SplitTypeName(this string name)
    {
        ArgumentNullException.ThrowIfNull(name);

        // Regex to split on transitions: lowercase→uppercase, acronym→normal, letter→digit, digit→letter
        var parts = TypeNameFormaterRegex().Matches(name)
                         ?? throw new InvalidOperationException("Regex failed");

        return string.Join(" ", parts);
    }

    [GeneratedRegex(@"([A-Z]+(?=$|[A-Z][a-z0-9])|[A-Z]?[a-z0-9]+)")]
    private static partial Regex TypeNameFormaterRegex();

}

