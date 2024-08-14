
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
using Xpandables.Net.Api.Persons.Persistence;
using Xpandables.Net.Distribution;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Features.GetPerson;

public sealed class GetPersonQueryHandler(
    DatabasePerson database) :
    IRequestHandler<GetPersonQuery, PersonResponse>
{
    public async Task<IOperationResult<PersonResponse>> HandleAsync(
        GetPersonQuery request,
        CancellationToken cancellationToken)
    {
        await Task.Yield();

        var person = database.FindById(request.PersonId);

        if (person is not null)
        {
            PersonResponse response = new()
            {
                PersonId = person.KeyId,
                FirstName = person.FirstName,
                LastName = person.LastName,
                ContactIds = person.Contacts
                    .Select(x => x.KeyId)
                    .ToList()
            };

            return OperationResults
                .Ok(response)
                .Build();
        }

        return OperationResults
            .NotFound<PersonResponse>()
            .WithError(nameof(request.PersonId), "Person not found")
            .Build();
    }
}
