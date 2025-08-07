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

namespace Xpandables.Net.Repositories.Sql;

/// <summary>
/// Provides factory methods for creating SQL builders with a unified API.
/// </summary>
public static class SqlBuilder
{
    /// <summary>
    /// Creates a new stored procedure builder for the specified procedure name.
    /// </summary>
    /// <param name="procedureName">The name of the stored procedure to execute.</param>
    /// <returns>A new instance of <see cref="StoredProcedureBuilder"/> configured for the specified procedure.</returns>
    /// <exception cref="ArgumentException">Thrown when the procedure name is null or empty.</exception>
    public static StoredProcedureBuilder StoredProcedure(string procedureName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(procedureName);
        return new StoredProcedureBuilder(procedureName);
    }
}