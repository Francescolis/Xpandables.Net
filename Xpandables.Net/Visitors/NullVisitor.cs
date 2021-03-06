﻿
/************************************************************************************************************
 * Copyright (C) 2020 Francis-Black EWANE
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
using System.Threading;
using System.Threading.Tasks;

namespace Xpandables.Net.Visitors
{
    /// <summary>
    /// Visitor when no explicit registration exist for a given type.
    /// </summary>
    /// <typeparam name="TElement">Type of element to be visited.</typeparam>
    public sealed class NullVisitor<TElement> : IVisitor<TElement>
        where TElement : class, IVisitable<TElement>
    {
        /// <summary>
        /// Does nothing.
        /// </summary>
        public async Task VisitAsync(TElement element, CancellationToken cancellationToken = default) => await Task.CompletedTask.ConfigureAwait(false);
    }
}