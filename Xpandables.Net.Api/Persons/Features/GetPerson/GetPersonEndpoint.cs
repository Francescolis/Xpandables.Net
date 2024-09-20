
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
using Microsoft.AspNetCore.Mvc;

using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Features.GetPerson;

public sealed class GetPersonEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
        => app.MapGet(
            ContractEndpoint.PersonGetEndpoint,
            async (
                [FromRoute] Guid keyId,
                IDispatcher dispatcher,
                CancellationToken cancellationToken) =>
            {
                GetPersonQuery query = new(keyId);

                return await dispatcher
                    .GetAsync(query, cancellationToken)
                    .ConfigureAwait(false);
            })
            .WithTags(ContractEndpoint.PersonEndpoint)
            .WithName(nameof(GetPersonEndpoint))
            .WithXValidatorFilter()
            .WithXOperationResultFilter()
            .AllowAnonymous()
            .Produces200OK()
            .Produces400BadRequest()
            .Produces401Unauthorized()
            .Produces500InternalServerError();
}
