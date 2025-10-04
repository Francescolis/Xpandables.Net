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
namespace Xpandables.Net.Abstractions.States;

/// <summary>
/// Represents an originator that can save and restore its state.
/// </summary>
public interface IOriginator
{
    /// <summary>
    /// Saves the current state of the originator.
    /// </summary>
    /// <returns>A memento containing the saved state.</returns>
    IMemento Save();

    /// <summary>
    /// Restores the state of the originator from the given memento.
    /// </summary>
    /// <param name="memento">The memento containing the state to restore.</param>
    void Restore(IMemento memento);
}
