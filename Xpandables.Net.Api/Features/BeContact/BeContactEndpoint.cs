
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

using Xpandables.Net.Commands;
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Features.BeContact;

public sealed class BeContactEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost(
               ContractEndpoint.PersonRegisterContactEndpoint,
               async (
                   [FromRoute] Guid keyId,
                   [FromBody] BeContactRequest request,
                   IDispatcher dispatcher,
                   CancellationToken cancellationToken) =>
               {
                   BeContactCommand command = new()
                   {
                       KeyId = keyId,
                       ContactId = request.ContactId
                   };

                   return await dispatcher
                       .SendAsync(command, cancellationToken)
                       .ConfigureAwait(false);
               })
               .WithTags(ContractEndpoint.PersonEndpoint)
               .WithName(nameof(BeContactEndpoint))
               .WithXValidatorFilter()
               .WithXOperationResultFilter()
               .AllowAnonymous()
               .Accepts<BeContactRequest>()
               .Produces200OK()
               .Produces409Conflict()
               .Produces400BadRequest()
               .Produces401Unauthorized()
               .Produces500InternalServerError();
}
