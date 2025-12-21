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

using System.Entities.Data.Converters;
using System.Events.Data;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
/// Customizes the EF Core model for the outbox store when using SQL Server, applying configuration specific to
/// integration event entities.
/// <code>
///     How to use it :
///     
///     services.Replace(ServiceDescriptor.Singleton&lt;IModelCustomizer, OutboxStoreSqlServerModelCustomizer&gt;());
///     
///     Or directly in the dbContext registration:
///     
///     services.AddDbContext&lt;OutboxStoreDataContext&gt;(options =&gt;
///         options.UseSqlServer(connectionString)
///                .ReplaceService&lt;IModelCustomizer, OutboxStoreSqlServerModelCustomizer&gt;());
/// </code>
/// </summary>
/// <remarks>This customizer configures the EntityIntegrationEvent entity to use a JSON value converter for the
/// EventData property, ensuring proper serialization and storage in SQL Server. It should be used in scenarios where
/// outbox pattern support for integration events is required in a SQL Server-backed context.</remarks>
/// <param name="dependencies">The set of dependencies required for model customization operations.</param>
public class OutboxStoreSqlServerModelCustomizer(ModelCustomizerDependencies dependencies) : ModelCustomizer(dependencies)
{
    /// <inheritdoc/>
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);

        modelBuilder.Entity<EntityIntegrationEvent>()
            .Property(e => e.EventData)
            .HasConversion<JsonDocumentValueConverter>();

        base.Customize(modelBuilder, context);
    }
}