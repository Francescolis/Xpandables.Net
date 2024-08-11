
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
using Xpandables.Net.Api.Domains;
using Xpandables.Net.Api.Primitives;
using Xpandables.Net.Commands;
using Xpandables.Net.Primitives;

namespace Xpandables.Net.Api.Features.RegisterPerson;

// we are using the decider pattern
public sealed record RegisterPersonCommand : Command<Person>, IValidateDecorator
{
    public required FirstName FirstName { get; init; }
    public required LastName LastName { get; init; }
    public override bool ContinueWhenNotFound => true;
}
