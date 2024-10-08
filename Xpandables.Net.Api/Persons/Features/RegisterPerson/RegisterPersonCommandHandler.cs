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
using Xpandables.Net.Api.Persons.Domains;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Features.RegisterPerson;

// we are using the decider pattern
public sealed class RegisterPersonCommandHandler :
    IRequestAggregateHandler<RegisterPersonCommand, Person>
{
    public async Task<IOperationResult> HandleAsync(
        RegisterPersonCommand command,
        CancellationToken cancellationToken = default)
    {
        await Task.Yield();

        if (command.Aggregate.IsNotEmpty)
        {
            return OperationResults
                .Conflict()
                .WithError(nameof(command.KeyId), "Person already exist")
                .Build();
        }

        command.Aggregate = Person
            .Create(command.KeyId, command.FirstName, command.LastName);

        return OperationResults
             .Ok()
             .WithHeader(nameof(PersonId), command.KeyId.ToString())
             .Build();
    }
}
