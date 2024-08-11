
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
using Xpandables.Net.Api.Primitives;
using Xpandables.Net.Http;
using Xpandables.Net.Primitives;

using static Xpandables.Net.Http.HttpClientParameters;

namespace Xpandables.Net.Api.Features.RegisterPerson;

[HttpClient(Path = ContractEndpoint.PersonRegisterEndpoint,
    IsNullable = false,
    IsSecured = false,
    Location = Location.Body,
    Method = Method.POST)]
public sealed record RegisterPersonRequest :
    IHttpClientRequest, IHttpRequestString, IValidateDecorator
{
    public required Guid KeyId { get; init; }
    [FirstNameFormat]
    public required string FirstName { get; init; }
    [LastNameFormat]
    public required string LastName { get; init; }

    object IHttpRequestString.GetStringContent() => new { FirstName, LastName };
}
