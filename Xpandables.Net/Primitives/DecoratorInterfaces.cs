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
using Xpandables.Net.Operations;

namespace Xpandables.Net.Primitives;

/// <summary>
/// A marker interface that allows classes that act like decorator, to be
/// recognized as well and not to be registered as normal implementations.
/// </summary>
#pragma warning disable CA1040 // Avoid empty interfaces
public interface IDecorator
{
}

/// <summary>
/// A marker interface that allows the class implementation to be intercepted.
/// You need to register the expected behavior using the appropriate 
/// interceptor extension method and provide 
/// an implementation for <see langword="IInterceptor"/>.
/// </summary>
public interface IInterceptorDecorator { }


/// <summary>
/// Defines a marker interface that allows the command/query class 
/// to add correlation decorator context result after a control flow.
/// In the calls handling the query/command, using the
/// <see cref="IOperationFinalizer"/>
/// </summary>
public interface IOperationFinalizerDecorator { }

/// <summary>
/// A marker interface that allows a handler class 
/// implementation to use persistence data across the control flow.
/// </summary>
public interface IPersistenceDecorator { }

/// <summary>
/// A marker interface that allows the command handler
/// class implementation to be decorated with transaction behavior 
/// according to the decorated class type.
/// </summary>
public interface ITransactionDecorator { }

/// <summary>
/// A marker interface that allows the command/query/request class to 
/// be decorated with the validation behavior according to the class type.
/// </summary>
public interface IValidateDecorator
{
}

/// <summary>
/// A marker interface that allows the command/query/request class 
/// to be decorated with the visitor behavior according to the class type.
/// </summary>
public interface IVisitorDecorator
{
}

#pragma warning restore CA1040 // Avoid empty interfaces