
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
using System.Reflection;

using Xpandables.Net.Primitives.I18n;

namespace Xpandables.Net.Composition;

/// <summary>
/// Defines options to configure export services.
/// </summary>
public sealed record class ExportServiceOptions
{
    /// <summary>
    /// Initializes a default instance of <see cref="ExportServiceOptions"/> class.
    /// </summary>
    public ExportServiceOptions() { }

    /// <summary>
    /// Gets or sets the path to the directory to scan for assemblies to add to the catalog.
    /// if not defined, the system will look to the application current directory.
    /// </summary>
    public string Path { get; set; } = GetPath();

    /// <summary>
    /// Gets or sets the pattern to search with. The format of the pattern should be the same as specified for GetFiles.
    /// If not defined, the system will use the <see langword="*.dll"/> pattern.
    /// </summary>
    public string SearchPattern { get; set; } = "*.dll";

    /// <summary>
    /// Gets or sets whether or not to search in sub-directories.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SearchSubDirectories { get; set; }

    private static string GetPath()
    {
        if (Assembly.GetEntryAssembly() is Assembly assembly)
            if (System.IO.Path.GetDirectoryName(assembly.Location) is string directoryName)
                return directoryName;

        throw new InvalidOperationException(I18nXpandables.PathAssemblyUnavailable);
    }
}
