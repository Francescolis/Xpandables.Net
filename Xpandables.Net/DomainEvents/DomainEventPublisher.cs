﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Xpandables.Net.Handlers;

namespace Xpandables.Net.DomainEvents
{
    /// <summary>
    /// The domain event publisher.
    /// </summary>
    public class DomainEventPublisher : IDomainEventPublisher
    {
        private readonly IHandlerAccessor _handlerAccessor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DomainEventPublisher"/> class with the handlers provider.
        /// </summary>
        /// <param name="handlerAccessor">The handlers provider.</param>
        /// <exception cref="ArgumentNullException">The <paramref name="handlerAccessor"/> is null.</exception>
        public DomainEventPublisher(IHandlerAccessor handlerAccessor)
            => _handlerAccessor = handlerAccessor ?? throw new ArgumentNullException(nameof(handlerAccessor));

        ///<inheritdoc/>
        public virtual async Task PublishAsync(IEvent @event, CancellationToken cancellationToken = default)
        {
            _ = @event ?? throw new ArgumentNullException(nameof(@event));

            var genericInterface = @event.GetType().GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEvent<>))
                ?? throw new ArgumentException($"The type '{@event.GetType().Name}' must implement '{typeof(IDomainEvent<>).Name}' interface.");
            var aggregateIdType = genericInterface.GetGenericArguments()[0];

            var genericHandlerType = typeof(IDomainEventHandler<,>);

            if (!genericHandlerType.TryMakeGenericType(out var typeHandler, out var typeException, aggregateIdType, @event.GetType()))
            {
                Trace.WriteLine(new InvalidOperationException("Building domain event Handler type failed.", typeException));
                return;
            }

            if (!_handlerAccessor.TryGetHandlers(typeHandler, out var foundHandlers, out var ex))
            {
                Trace.WriteLine(new InvalidOperationException($"Matching domain event handlers for {@event.GetType().Name} are missing.", ex));
                return;
            }

            if (!foundHandlers.Any())
                return;

            var handlers = (IEnumerable<IDomainEventHandler>)foundHandlers;
            var tasks = handlers.Select(handler => handler.HandleAsync(@event, cancellationToken));

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
    }
}
