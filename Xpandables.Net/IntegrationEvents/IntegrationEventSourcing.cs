
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
using Xpandables.Net.Primitives.I18n;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.IntegrationEvents;

internal sealed class IntegrationEventSourcing : IIntegrationEventSourcing
{
    private readonly List<IIntegrationEvent> _events = [];
    public void Append(IIntegrationEvent @event)
    {
        ArgumentNullException.ThrowIfNull(@event);

        if (_events.Exists(e => e.Id == @event.Id))
            throw new InvalidOperationException(
                I18nXpandables.EventSourcingNotificationAlreadyExists
                .StringFormat(@event.Id));

        _events.Add(@event);
    }

    public IEnumerable<IIntegrationEvent> GetIntegrationEvents()
        => _events.OrderBy(o => o.OccurredOn);

    public void MarkIntegrationEventsAsCommitted() => _events.Clear();
}
