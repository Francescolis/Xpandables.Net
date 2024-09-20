
/*******************************************************************************
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
********************************************************************************/
using Xpandables.Net.Decorators;
using Xpandables.Net.Operations;
using Xpandables.Net.Transactions;
using Xpandables.Net.Visitors;

namespace Xpandables.Net;

/// <summary>
/// Defines options to apply decorators to request handlers.
/// </summary>
public sealed record class RequestOptions
{
    /// <summary>
    /// Enables Validator behavior to requests 
    /// that are decorated with the <see cref="IValidateDecorator"/> interface.
    /// </summary>
    public RequestOptions UseValidator()
    {
        IsValidatorEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables visitor behavior to requests that 
    /// implement the <see cref="IVisitable{TVisitable}"/> interface.
    /// </summary>
    public RequestOptions UseVisitor()
    {
        IsVisitorEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables persistence behavior to requests 
    /// that are decorated with the <see cref="IPersistenceDecorator"/> 
    /// interface.
    /// </summary>
    public RequestOptions UsePersistence()
    {
        IsPersistenceEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables operation result finalizer behavior to requests/queries 
    /// that are decorated with the 
    /// <see cref="IOperationFinalizerDecorator"/> interface.
    /// </summary>
    /// <remarks>The target implementation handler(s) must reference 
    /// the <see cref="IOperationFinalizer"/> in order to configure 
    /// the result.</remarks>
    public RequestOptions UseOperationFinalizer()
    {
        IsOperationFinalizerEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables transaction behavior to requests that are decorated 
    /// with the <see cref="ITransactionDecorator"/> interface.
    /// You must register a definition for
    /// <see cref="ITransactional"/>
    /// that provides with the transactional behavior.
    /// </summary>
    public RequestOptions UseTransaction()
    {
        IsTransactionEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables aggregate behavior to <see cref="IRequestAggregate{TAggregate}"/>
    /// providing with the ambient aggregate for the request.
    /// </summary>
    public RequestOptions UseAggregate()
    {
        IsAggregateEnabled = true;
        return this;
    }

    /// <summary>
    /// Enables duplicate event behavior to events that are decorated
    /// with the <see cref="IEventDuplicateDecorator"/> interface.
    /// </summary>
    public RequestOptions UseDuplicateEvent()
    {
        IsDuplicateEventEnabled = true;
        return this;
    }

    internal bool IsValidatorEnabled { get; private set; }
    internal bool IsVisitorEnabled { get; private set; }
    internal bool IsTransactionEnabled { get; private set; }
    internal bool IsPersistenceEnabled { get; private set; }
    internal bool IsAggregateEnabled { get; private set; }
    internal bool IsDuplicateEventEnabled { get; private set; }
    internal bool IsOperationFinalizerEnabled { get; private set; }
}
