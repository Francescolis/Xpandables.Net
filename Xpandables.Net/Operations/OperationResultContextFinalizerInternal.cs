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
namespace Xpandables.Net.Operations;

/// <summary>
/// Defines a marker interface that allows the command/query class 
/// to add correlation decorator context result after a control flow.
/// In the calls handling the query/command, 
/// you should reference the <see cref="IOperationResultContextFinalizer"/> and set 
/// the <see cref="IOperationResultContextFinalizer.Finalizer"/>.
/// </summary>
public interface IOperationResultContextDecorator { }

/// <summary>
/// Defines a delegate that allows to finalize the operation result.
/// </summary>
/// <param name="operationResult">The operation result to act on.</param>
/// <returns>An instance of <see cref="OperationResult"/>.</returns>
public delegate OperationResult OperationResultContextFinalizer(OperationResult operationResult);

/// <summary>
/// Defines the action tat get applied and the end of a process before the result is returned to the caller.
/// In order to be activated, the target class should implement 
/// the <see cref="IOperationResultContextDecorator"/> interface, 
/// the target handling class should reference the current interface to set the delegate.
/// </summary>
public interface IOperationResultContextFinalizer
{
    /// <summary>
    /// Applies finalizer to the operation result.
    /// </summary>
    OperationResultContextFinalizer? Finalizer { get; set; }
}
