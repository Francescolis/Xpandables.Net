
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
using Microsoft.EntityFrameworkCore;

using Xpandables.Net.EntityFramework.Repositories;

namespace Xpandables.Net.Repositories;

/// <summary>
/// Represents the base EFCore implementation of <see cref="IUnitOfWork"/>.
/// </summary>
/// <remarks>
/// Constructs a new instance of <see cref="UnitOfWork"/>.
/// </remarks>
/// <param name="context">The db context to act with.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="context"/> is null.</exception>
public class UnitOfWork(DataContext context) : Disposable, IUnitOfWork
{
    /// <summary>
    /// Gets the current <see cref="DataContext"/> instance.
    /// </summary>
    protected DataContext Context { get; } = context;

    ///<inheritdoc/>
    public async Task PersistAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await Context
                .SaveChangesAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
            when (exception is DbUpdateException or DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                $"Save changes failed. See inner exception.",
                exception);
        }
    }

    private bool _isDisposed;

    ///<inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            if (disposing)
            {
                Context?.Dispose();
            }

            base.Dispose(disposing);
        }
    }

    ///<inheritdoc/>
    protected override async ValueTask DisposeAsync(bool disposing)
    {
        if (!_isDisposed)
        {
            _isDisposed = true;

            if (disposing)
            {
                await Context.DisposeAsync().ConfigureAwait(false);
            }

            await base.DisposeAsync(disposing).ConfigureAwait(false);
        }
    }

    ///<inheritdoc/>
    public virtual IRepositoryRead<TEntity> GetRepositoryRead<TEntity>()
        where TEntity : class
        => new RepositoryReadDecorator<TEntity>(
            new RepositoryRead<TEntity>(Context));

    ///<inheritdoc/>
    public virtual IRepositoryWrite<TEntity> GetRepositoryWrite<TEntity>()
        where TEntity : class
        => new RepositoryWriteDecorator<TEntity>(
            new RepositoryWrite<TEntity>(Context));

    ///<inheritdoc/>
    public virtual IRepository<TEntity> GetRepository<TEntity>()
        where TEntity : class
        => new Repository<TEntity>(
            GetRepositoryRead<TEntity>(),
            GetRepositoryWrite<TEntity>());
}

/// <summary>
/// Represents the base EFCore implementation 
/// of <see cref="IUnitOfWork{TDataContext}"/>.
/// </summary>
/// <typeparam name="TDataContext">The type of the context 
/// that implements <see cref="DataContext"/>.</typeparam>
/// <remarks>
/// Constructs a new instance of <see cref="UnitOfWork{TContext}"/>.
/// </remarks>
/// <param name="context">The db context to act with.</param>
/// <exception cref="ArgumentNullException">The 
/// <paramref name="context"/> is null.</exception>
public class UnitOfWork<TDataContext>(TDataContext context)
    : UnitOfWork(context), IUnitOfWork<TDataContext>
    where TDataContext : DataContext
{
    /// <summary>
    /// Gets the current <typeparamref name="TDataContext"/> instance.
    /// </summary>
    protected new TDataContext Context { get; } = context;

    ///<inheritdoc/>
    public override IRepositoryRead<TEntity, TDataContext>
        GetRepositoryRead<TEntity>()
        => new RepositoryReadDecorator<TEntity, TDataContext>(
            new RepositoryRead<TEntity, TDataContext>(Context));

    ///<inheritdoc/>
    public override IRepositoryWrite<TEntity, TDataContext>
        GetRepositoryWrite<TEntity>()
        => new RepositoryWriteDecorator<TEntity, TDataContext>(
            new RepositoryWrite<TEntity, TDataContext>(Context));

    ///<inheritdoc/>
    public override IRepository<TEntity> GetRepository<TEntity>()
        => new Repository<TEntity, TDataContext>(
            GetRepositoryRead<TEntity>(),
            GetRepositoryWrite<TEntity>());
}