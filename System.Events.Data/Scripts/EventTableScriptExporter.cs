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
using System.Text;

namespace System.Events.Data.Scripts;

/// <summary>
/// Exports event table scripts to disk.
/// </summary>
public static class EventTableScriptExporter
{
    /// <summary>
    /// Exports the given scripts to a target directory.
    /// </summary>
    /// <param name="provider">Script provider.</param>
    /// <param name="directory">Target directory.</param>
    /// <param name="schema">Schema name.</param>
    public static void ExportScripts(IEventTableScriptProvider provider, string directory, string schema = "Events")
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);

        Directory.CreateDirectory(directory);

        File.WriteAllText(Path.Combine(directory, "CreateEventTables.sql"),
            provider.GetCreateAllTablesScript(schema), Encoding.UTF8);
        File.WriteAllText(Path.Combine(directory, "DropEventTables.sql"),
            provider.GetDropAllTablesScript(schema), Encoding.UTF8);
    }
}
