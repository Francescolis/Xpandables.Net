
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
namespace Xpandables.Net.Primitives.Text;

/// <summary>
/// Defines a method to send a message.
/// </summary>
public interface ITextMessenger
{
    /// <summary>
    /// Sends the specified message according.
    /// </summary>
    /// <param name="message">The message instance.</param>
    /// <param name="cancellationToken">A CancellationToken to observe
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Unable to send the 
    /// message. See inner exception.</exception>
    Task SendAsync(
        object message,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Defines a method to send a specific message.
/// </summary>
/// <typeparam name="TMessage">The type of the message content.</typeparam>
public interface ITextMessenger<in TMessage> : ITextMessenger
    where TMessage : notnull
{
    /// <summary>
    /// Sends the specified message according to its type.
    /// </summary>
    /// <param name="message">The message instance.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Unable to send the 
    /// message. See inner exception.</exception>
    Task SendAsync(
        TMessage message,
        CancellationToken cancellationToken = default);

    Task ITextMessenger.SendAsync(
        object message,
        CancellationToken cancellationToken)
        => SendAsync((TMessage)message, cancellationToken);
}
