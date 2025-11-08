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
using Xpandables.Net.AsyncPaged;

namespace Xpandables.Net.Benchmarking;

public sealed class SamplePagedEnumerable : IAsyncPagedEnumerable<string>
{
    public Pagination Pagination => Pagination.Create(100, 1000, null);

    public async IAsyncEnumerator<string> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        for (int i = 0; i < 1000; i++)
        {
            yield return $"Item {i}";
            await Task.Yield();
        }
    }

    public Task<Pagination> GetPaginationAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(Pagination);
}