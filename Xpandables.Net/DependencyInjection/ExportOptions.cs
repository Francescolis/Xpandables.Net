
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

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Represents the options for exporting dependencies.
/// </summary>
public sealed record class ExportOptions
{
    /// <summary>
    /// Gets or sets the path where the dependencies are exported.
    /// </summary>
    /// <remarks>if not defined, the system will look to the application 
    /// current directory.</remarks>
    public string Path { get; set; } = GetPath();

    /// <summary>
    /// Gets or sets the search pattern for finding dependencies.
    /// </summary>
    /// <remarks>The format of the pattern should be the same as specified 
    /// for GetFiles. If not defined, the system will use the 
    /// <see langword="*.dll"/> pattern.</remarks>
    public string SearchPattern { get; set; } = "*.dll";

    /// <summary>
    /// Gets or sets a value indicating whether to search subdirectories 
    /// for dependencies.
    /// </summary>
    /// <remarks>The default value is <see langword="false"/>.</remarks>
    public bool SearchSubDirectories { get; set; }

    private static string GetPath()
    {
        if (Assembly.GetEntryAssembly() is Assembly assembly)
        {
            if (System.IO.Path
                .GetDirectoryName(assembly.Location) is string directoryName)
            {
                return directoryName;
            }
        }

        throw new InvalidOperationException(
            "The entry assembly location is not found.");
    }
}
