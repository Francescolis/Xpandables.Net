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
using System.Diagnostics;

using Xpandables.Net.Api.Persons.Domains;
using Xpandables.Net.Api.Persons.Repositories;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Features.BeContact;

public sealed class BeContactCommandHandler(IPersonExistChecker checker) :
    IRequestAggregateHandler<BeContactCommand, Person>
{
    public async Task<IOperationResult> HandleAsync(
        BeContactCommand command,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();
        Debug.Assert(command.Aggregate.IsNotEmpty);

        IOperationResult result = command
            .Aggregate
            .Value
            .BeContact(command.ContactId, checker);

        return result;
    }
}
