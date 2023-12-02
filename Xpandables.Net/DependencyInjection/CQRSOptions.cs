
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
using Xpandables.Net.CQRS.Decorators;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives;
using Xpandables.Net.Validators;
using Xpandables.Net.Visitors;

namespace Xpandables.Net.DependencyInjection;

/// <summary>
/// Defines options to apply decorators to handlers on registration.
/// </summary>
public sealed record class CQRSOptions
{
    /// <summary>
    /// Enables Validator behavior to commands/queries 
    /// that are decorated with the <see cref="IValidateDecorator"/> interface.
    /// </summary>
    public CQRSOptions UseValidator()
    {
        IsValidatorEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables visitor behavior to commands/queries that 
    /// implement the <see cref="IVisitable{TVisitable}"/> interface.
    /// </summary>
    public CQRSOptions UseVisitor()
    {
        IsVisitorEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables persistence behavior to commands 
    /// that are decorated with the <see cref="IPersistenceDecorator"/> interface.
    /// </summary>
    public CQRSOptions UsePersistence()
    {
        IsPersistenceEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables operation result finalizer behavior to commands/queries 
    /// that are decorated with the <see cref="IOperationResultDecorator"/> interface.
    /// </summary>
    /// <remarks>The target implementation handler(s) must reference 
    /// the <see cref="IOperationResultFinalizer"/> in order to configure the result.</remarks>
    public CQRSOptions UseOperationResult()
    {
        IsOperationResultEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables transaction behavior to commands that are decorated 
    /// with the <see cref="ITransactionDecorator"/> interface.
    /// You must register a definition for <see cref="TransactionCommandHandler"/> 
    /// that provides with the transactional behavior.
    /// </summary>
    public CQRSOptions UseTransaction()
    {
        IsTransactionEnabled = true;
        return this;
    }

    internal bool IsValidatorEnabled { get; private set; }
    internal bool IsVisitorEnabled { get; private set; }
    internal bool IsTransactionEnabled { get; private set; }
    internal bool IsPersistenceEnabled { get; private set; }
    internal bool IsOperationResultEnabled { get; private set; }
}
