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

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents a unit of work that encapsulates a set of operations to be 
/// performed on a data context.
/// </summary>
public class UnitOfWork(DataContext context, IServiceProvider serviceProvider) :
    UnitOfWorkCore
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    /// <summary>
    /// Gets the data context associated with this unit of work.
    /// </summary>
    protected DataContext Context { get; } = context;

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is not InvalidOperationException)
        {
            throw new InvalidOperationException(
                "An error occurred while saving the changes.",
                exception);
        }
    }

    /// <inheritdoc/>
    protected override IRepository GetRepositoryCore(Type repositoryType) =>
        _serviceProvider.GetService(repositoryType) as IRepository
            ?? throw new InvalidOperationException(
                $"The repository of type {repositoryType.Name} is not registered.");

    /// <inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (disposing)
        {
            await Context.DisposeAsync().ConfigureAwait(false);
        }

        await base.DisposeAsync(disposing).ConfigureAwait(false);
    }
}
