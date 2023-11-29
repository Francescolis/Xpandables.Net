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
namespace Xpandables.Net.Primitives.Text;

/// <summary>
/// Allows an application author to send messages.
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Asynchronously sends the specified message according to its type.
    /// </summary>
    /// <param name="message">The message instance.</param>
    /// <param name="cancellationToken">A CancellationToken to observe
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Unable to send the message.</exception>
    ValueTask SendAsync(object message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Allows an application author to send messages of specific type.
/// </summary>
/// <typeparam name="TMessage">The type of the message content.</typeparam>
public interface IMessageSender<in TMessage> : IMessageSender
    where TMessage : notnull
{
    /// <summary>
    /// Asynchronously sends the specified message according to its type.
    /// </summary>
    /// <param name="message">The message instance.</param>
    /// <param name="cancellationToken">A CancellationToken to observe 
    /// while waiting for the task to complete.</param>
    /// <returns>A task that represents an asynchronous operation.</returns>
    /// <exception cref="InvalidOperationException">Unable to send the message.</exception>
    ValueTask SendAsync(TMessage message, CancellationToken cancellationToken = default);

    ValueTask IMessageSender.SendAsync(object message, CancellationToken cancellationToken)
        => SendAsync((TMessage)message, cancellationToken);
}
