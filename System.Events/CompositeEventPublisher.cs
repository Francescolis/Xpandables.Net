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
namespace System.Events;

/// <summary>
/// Publishes events using multiple underlying <see cref="IEventPublisher"/> implementations.
/// </summary>
/// <remarks>
/// This is useful when you want to publish the same event to multiple targets (e.g. in-process handlers + external bus).
/// </remarks>
public sealed class CompositeEventPublisher(IEnumerable<IEventPublisher> publishers) : IEventPublisher
{
    private readonly IEventPublisher[] _publishers = [.. (publishers ?? throw new ArgumentNullException(nameof(publishers)))
        .Where(static p => p is not CompositeEventPublisher)];

    /// <inheritdoc/>
    public async Task PublishAsync<TEvent>(TEvent eventInstance, CancellationToken cancellationToken = default)
        where TEvent : class, IEvent
    {
        ArgumentNullException.ThrowIfNull(eventInstance);

        if (_publishers.Length == 0)
        {
            return;
        }

        var tasks = new Task[_publishers.Length];

        for (int i = 0; i < _publishers.Length; i++)
        {
            tasks[i] = _publishers[i].PublishAsync(eventInstance, cancellationToken);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }
}
