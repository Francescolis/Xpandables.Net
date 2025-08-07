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

using System.Data;
using System.Data.Common;

namespace Xpandables.Net.Repositories.Sql;

/// <summary>
/// A generic data adapter implementation for filling DataSets with database command results.
/// </summary>
internal sealed class GenericDataAdapter : DbDataAdapter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GenericDataAdapter"/> class.
    /// </summary>
    /// <param name="selectCommand">The select command to execute.</param>
    public GenericDataAdapter(DbCommand selectCommand)
    {
        SelectCommand = selectCommand;
    }

    /// <summary>
    /// Fills the specified DataSet with data using the select command.
    /// </summary>
    /// <param name="dataSet">The DataSet to fill.</param>
    /// <returns>The number of rows successfully added or refreshed in the DataSet.</returns>
    public override int Fill(DataSet dataSet)
    {
        ArgumentNullException.ThrowIfNull(dataSet);
        
        if (SelectCommand is null)
            return 0;

        int totalRows = 0;
        int tableIndex = 0;

        using DbDataReader reader = SelectCommand.ExecuteReader();
        
        do
        {
            DataTable table = new($"Table{tableIndex}");
            
            // Create columns based on the reader schema
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);
                Type fieldType = reader.GetFieldType(i);
                
                // Handle nullable types
                Type columnType = fieldType;
                if (fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) is null)
                {
                    columnType = typeof(Nullable<>).MakeGenericType(fieldType);
                }
                
                table.Columns.Add(columnName, Nullable.GetUnderlyingType(columnType) ?? columnType);
            }

            // Fill the table with data
            int rowCount = 0;
            while (reader.Read())
            {
                object[] values = new object[reader.FieldCount];
                reader.GetValues(values);
                
                // Convert DBNull to null
                for (int i = 0; i < values.Length; i++)
                {
                    if (values[i] == DBNull.Value)
                        values[i] = null!;
                }
                
                table.Rows.Add(values);
                rowCount++;
            }

            if (table.Columns.Count > 0)
            {
                dataSet.Tables.Add(table);
                totalRows += rowCount;
                tableIndex++;
            }
            
        } while (reader.NextResult());

        return totalRows;
    }
}