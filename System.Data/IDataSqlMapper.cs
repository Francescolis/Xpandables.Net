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
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace System.Data;

/// <summary>
/// Defines a contract for mapping data from a database reader to a specified result type.
/// </summary>
/// <remarks>Implementations of this interface should provide the logic to convert data from the provided
/// DbDataReader into the desired TResult type. This is useful for data access layers that need to transform raw
/// database results into application-specific objects.</remarks>
public interface IDataSqlMapper
{
    /// <summary>
    /// Maps the current record from the specified <see cref="DbDataReader"/> to an instance of the specified type.
    /// </summary>
    /// <remarks>Ensure that the reader is positioned at a valid record before calling this method. The
    /// mapping assumes that the reader contains all required fields for the target type.</remarks>
    /// <typeparam name="TResult">The type of object to which the data from the reader will be mapped.</typeparam>
    /// <param name="reader">The <see cref="DbDataReader"/> containing the data to be mapped. The reader must be positioned at a valid record
    /// and not be closed.</param>
    /// <returns>An instance of <typeparamref name="TResult"/> populated with the data from the current record of the reader.</returns>
    TResult MapToResult<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(DbDataReader reader);

    /// <summary>
    /// Maps data from the specified data reader to a result of the given type according to the provided data specification.
    /// </summary>
    /// <remarks>This method is typically used in data access scenarios to transform database records into
    /// domain-specific result types. The mapping behavior is determined by the provided specification, allowing for
    /// flexible and reusable data transformations.</remarks>
    /// <typeparam name="TData">The type of the data that the specification operates on. Must be a reference type.</typeparam>
    /// <typeparam name="TResult">The type of the result produced by the mapping process.</typeparam>
    /// <param name="specification">The data specification that defines how to map the data from the reader to the result type.</param>
    /// <param name="reader">The data reader that supplies the data to be mapped according to the specification.</param>
    /// <returns>An instance of TResult representing the mapped result based on the data from the reader.</returns>
    TResult MapToResult<TData, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor | DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.PublicProperties)] TResult>(
        IDataSpecification<TData, TResult> specification,
        DbDataReader reader)
        where TData : class;
}
