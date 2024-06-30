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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Operations;
using Xpandables.Net.Optionals;

namespace Xpandables.Net.Commands;

/// <summary>
/// This interface is used as a marker for commands targeting aggregate. 
/// It's used for implementing the Decider pattern. 
/// </summary>
public interface ICommand<TAggregate>
    where TAggregate : IAggregate
{
    /// <summary>
    /// Gets or sets the aggregate instance.
    /// </summary>
    /// <remarks>This get populated by the aspect.</remarks>
    Optional<TAggregate> Aggregate { get; set; }

    /// <summary>
    /// Gets the key aggretate identitifer
    /// </summary>
    Guid KeyId { get; }

    /// <summary>
    /// Gets the event identifier.
    /// </summary>
    public Guid Id => Guid.NewGuid();

    /// <summary>
    /// Gets When the event occurred.
    /// </summary>
    public DateTimeOffset OccurredOn => DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets the name of the user running associated with the current event.
    /// The default value is associated with the current thread.
    /// </summary>
    public string CreatedBy => Environment.UserName;
}

/// <summary>
/// Represents a method signature to be used to apply 
/// <see cref="ICommandHandler{TCommand}"/> implementation.
/// </summary>
/// <typeparam name="TAggregate">The type of aggregate.</typeparam>
/// <typeparam name="TCommand">Type of the command to act on.</typeparam>
/// <param name="command">The command instance to act on.</param>
/// <param name="cancellationToken">A CancellationToken to 
/// observe while waiting for the task to complete.</param>
/// <returns>A value that represents an <see cref="IOperationResult"/>.</returns>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="command"/> is null.</exception>
public delegate Task<IOperationResult> CommandHandler
    <TCommand, TAggregate>(
    TCommand command, CancellationToken cancellationToken = default)
    where TAggregate : class, IAggregate
    where TCommand : class, ICommand<TAggregate>;

/// <summary>
/// Provides with a method to handle commands that are associated with an 
/// aggregate using the Decider asynchronous command pattern.
/// </summary>
/// <typeparam name="TAggregate">The type of the aggregate.</typeparam>
/// <typeparam name="TCommand">The type of the aggregate 
/// command.</typeparam>
public interface ICommandHandler<TCommand, TAggregate>
    where TAggregate : class, IAggregate
    where TCommand : class, ICommand<TAggregate>
{
    /// <summary>
    /// Handles the specified command for the specified aggregate.
    /// </summary>
    /// <remarks>The target aggregate will be supplied by the aspect.</remarks>
    /// <param name="command">The command instance.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    Task<IOperationResult> HandleAsync(
        TCommand command,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a wrapper interface that avoids use of C# dynamics 
/// with decider pattern and allows 
/// type inference for <see cref="ICommandHandler{TCommand, TAggregate}"/>.
/// </summary>
/// <typeparam name="TAggregate">Type of the aggregate.</typeparam>
public interface ICommandHandlerWrapper<TAggregate>
    where TAggregate : class, IAggregate
{
    /// <summary>
    /// Asynchronously handles the specified command.
    /// </summary>
    /// <param name="command">The command to act on.</param>
    /// <param name="cancellationToken">A CancellationToken 
    /// to observe while waiting for the task to complete.</param>
    /// <exception cref="ArgumentNullException">The 
    /// <paramref name="command"/> is null.</exception>
    /// <returns>A task that represents an 
    /// object of <see cref="IOperationResult"/>.</returns>
    Task<IOperationResult> HandleAsync(
        ICommand<TAggregate> command,
        CancellationToken cancellationToken = default);
}