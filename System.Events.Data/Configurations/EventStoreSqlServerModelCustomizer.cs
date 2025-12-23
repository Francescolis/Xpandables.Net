/*******************************************************************************
 * Copyright (C) 2025 Kamersoft
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
namespace System.Events.Data.Configurations;

using System.Events.Data;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// Provides SQL Server-specific model customization for event store entities, configuring value conversion for event
/// data properties.
/// <code>
///     How to use it :
///     
///     services.Replace(ServiceDescriptor.Singleton&lt;IModelCustomizer, EventStoreSqlServerModelCustomizer&gt;());
///     
///     Or directly in the dbContext registration:
///     
///     services.AddDbContext&lt;EventStoreDataContext&gt;(options =&gt;
///         options.UseSqlServer(connectionString)
///                .ReplaceService&lt;IModelCustomizer, EventStoreSqlServerModelCustomizer&gt;());
/// </code>
/// </summary>
/// <remarks>This customizer configures the EntitySnapshotEvent and EntityDomainEvent types to use a JSON document
/// value converter for their EventData properties. This ensures that <see cref="JsonDocument"/> event data is stored and retrieved as JSON in SQL
/// Server. Use this customizer when working with event sourcing patterns that require serialization of event
/// payloads.</remarks>
/// <param name="dependencies">The set of dependencies required for model customization operations.</param>
public sealed class EventStoreSqlServerModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    /// <inheritdoc/>
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<EntitySnapshotEvent>()
            .Property(e => e.EventData)
            .HasEventJsonDocumentConversion();

        modelBuilder.Entity<EntityDomainEvent>()
            .Property(e => e.EventData)
            .HasEventJsonDocumentConversion();

        base.Customize(modelBuilder, context);
    }
}