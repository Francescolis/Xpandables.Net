/*******************************************************************************
 * Copyright (C) 2024 Francis-Black EWANE
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

using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Executions.Domains;

/// <summary>
/// Configuration class for the <see cref="EntitySnapshotEvent" /> entity.
/// </summary>
public sealed class EntitySnapShotEventTypeConfiguration : EntityEventTypeConfiguration<EntitySnapshotEvent>
{
    /// <inheritdoc />
    public sealed override void Configure(EntityTypeBuilder<EntitySnapshotEvent> builder)
    {
        base.Configure(builder);

        _ = builder.HasIndex(e => new { e.KeyId, e.OwnerId, e.Name, e.Sequence });
    }
}