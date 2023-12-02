/************************************************************************************************************
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
************************************************************************************************************/
using System.ComponentModel.Composition.Primitives;

namespace Xpandables.Net.Compositions;

/// <summary>
/// Defines a <see cref="ComposablePartCatalog"/> implementation 
/// that always returns an empty collection of parts.
/// </summary>
public sealed class EmptyCatalog : ComposablePartCatalog
{
    /// <summary>
    /// Gets the part definitions that are contained in the catalog.
    /// </summary>
    /// <returns>The <see cref="ComposablePartDefinition" /> contained in 
    /// the <see cref="ComposablePartCatalog" />.</returns>
    /// <exception cref="ObjectDisposedException">The <see cref="ComposablePartCatalog" /> 
    /// object has been disposed of.</exception>
    public override IQueryable<ComposablePartDefinition> Parts
        => Enumerable.Empty<ComposablePartDefinition>().AsQueryable();
}
