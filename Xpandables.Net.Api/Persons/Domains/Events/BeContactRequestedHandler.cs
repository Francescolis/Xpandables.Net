﻿
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

using Xpandables.Net.Distribution;
using Xpandables.Net.Events;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Domains.Events;

public sealed class BeContactRequestedHandler(
    IEventStore eventStore) : IEventHandler<BeContactRequested>
{
    public async Task<IOperationResult> HandleAsync(
        BeContactRequested @event,
        CancellationToken cancellationToken = default)
    {
        BeContactCompleted completed = new()
        {
            PersonId = @event.AggregateId,
            ContactId = @event.ContactId
        };

        return await eventStore
            .AppendEventAsync(completed, cancellationToken)
            .ToOperationResultAsync()
            .ConfigureAwait(false);
    }
}