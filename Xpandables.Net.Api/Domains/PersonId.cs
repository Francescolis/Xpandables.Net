
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
using Xpandables.Net.Aggregates;
using Xpandables.Net.Primitives.Converters;

namespace Xpandables.Net.Api.Domains;

[PrimitiveJsonConverter]
public readonly record struct PersonId(Guid Value) : IAggregateId<PersonId>
{
    public static PersonId Create(Guid value) => new(value);
    public static PersonId Default() => Create(Guid.Empty);

    public static implicit operator Guid(PersonId self) => self.Value;

    public static implicit operator string(PersonId self)
        => self.Value.ToString();
    public static implicit operator PersonId(Guid value) => new(value);
}