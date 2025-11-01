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
namespace Xpandables.Net.Events.Repositories;

/// <summary>
/// Defines a unit of work for event-related operations, providing method to persist changes to the underlying data
/// store.
/// </summary>
/// <remarks>Implementations of this interface coordinate the saving of changes made within a transactional scope.
/// Typically, changes are not committed until this method is called.</remarks>
public interface IEventUnitOfWork
{
    /// <summary>
    /// Saves all changes made in this unit of work.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The number of state entries written to the database.</returns>
    /// <exception cref="InvalidOperationException">All exceptions 
    /// related to the operation.</exception>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}