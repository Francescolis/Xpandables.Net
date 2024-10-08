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
using Xpandables.Net.DependencyInjection;
using Xpandables.Net.Operations;

namespace Xpandables.Net.Api.Persons.Features.RegisterPerson;

public sealed class RegisterPersonEndpoint : IEndpointRoute
{
    public void AddRoutes(IEndpointRouteBuilder app)
        => app.MapPost(
               ContractEndpoint.PersonRegisterEndpoint,
               async (
                   RegisterPersonRequest request,
                   IDispatcher dispatcher,
                   CancellationToken cancellationToken) =>
               {
                   RegisterPersonCommand command = new()
                   {
                       KeyId = request.KeyId,
                       FirstName = request.FirstName,
                       LastName = request.LastName
                   };

                   return await dispatcher
                       .SendAsync(command, cancellationToken)
                       .ConfigureAwait(false);
               })
               .WithTags(ContractEndpoint.PersonEndpoint)
               .WithName(nameof(RegisterPersonEndpoint))
               .WithXValidatorFilter()
               .WithXOperationResultFilter()
               .AllowAnonymous()
               .Accepts<RegisterPersonRequest>()
               .Produces200OK()
               .Produces409Conflict()
               .Produces400BadRequest()
               .Produces401Unauthorized()
               .Produces500InternalServerError();
}
