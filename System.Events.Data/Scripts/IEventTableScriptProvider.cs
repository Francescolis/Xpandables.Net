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
namespace System.Events.Data.Scripts;

/// <summary>
/// Provides SQL scripts for creating and dropping event store tables.
/// </summary>
public interface IEventTableScriptProvider
{
    /// <summary>
    /// Gets the SQL script to create all event tables.
    /// </summary>
    /// <param name="schema">Schema name for the tables.</param>
    /// <returns>The SQL script.</returns>
    string GetCreateAllTablesScript(string schema = "Events");

    /// <summary>
    /// Gets the SQL script to drop all event tables.
    /// </summary>
    /// <param name="schema">Schema name for the tables.</param>
    /// <returns>The SQL script.</returns>
    string GetDropAllTablesScript(string schema = "Events");
}
