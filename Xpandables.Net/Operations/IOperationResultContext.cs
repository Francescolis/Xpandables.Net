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
using Xpandables.Net.Optionals;

namespace Xpandables.Net.Operations;

/// <summary>
/// Defines a marker interface that allows the command/query class 
/// to add correlation decorator context result after a control flow.
/// In the calls handling the query/command, 
/// you should reference the <see cref="IOperationResultContext"/> and set 
/// the <see cref="IOperationResultContext.OnSuccess"/>
/// and/or <see cref="IOperationResultContext.OnFailure"/> according to the use.
/// <para>Note that if set, the <see cref="IOperationResultContext.OnSuccess"/> 
/// will be used in replacement of the current success operation result instance.
/// If set, the <see cref="IOperationResultContext.OnFailure"/> will 
/// also be used in replacement of the current failure operation result.</para>
/// </summary>
public interface IOperationResultDecorator { }

/// <summary>
/// Defines two operation results : one for success and one for failure 
/// that can be used in case of failure or success during the execution control flow.
/// if the execution is a failure, the <see cref="OnFailure"/> 
/// value will be used as result, otherwise the <see cref="OnSuccess"/> value will be used.
/// Only if those values are defined.
/// In order to be activated, the target class should implement 
/// the <see cref="IOperationResultDecorator"/> interface, 
/// the target handling class should reference the current interface to set the result(s).
/// </summary>
public interface IOperationResultContext
{
    /// <summary>
    /// Captures the operation result execution context 
    /// for a success operation result.
    /// When used in a control flow operation, 
    /// this value will be applied in case of successful execution operation only if it's defined.
    /// </summary>
    Optional<OperationResult> OnSuccess { get; set; }

    /// <summary>
    /// Captures the operation result execution context 
    /// for a failure operation result.
    /// When used in a control flow operation, 
    /// this value will be applied in case of failure execution operation only if it's defined.
    /// </summary>
    Optional<OperationResult> OnFailure { get; set; }
}
