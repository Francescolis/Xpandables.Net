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

using Xpandables.Net.Repositories;

namespace Xpandables.Net.Events;

public sealed class EventStore<TDataContext>(TDataContext context) : DisposableAsync, IEventStore
    where TDataContext : DataContext
{
    public Task AppendAsync(AppendRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task AppendSnapshotAsync(ISnapshotEvent snapshotEvent, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task DeleteStreamAsync(in DeleteStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<EnvelopeResult?> GetLatestSnapshotAsync(Guid ownerId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(in ReadAllStreamsRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(in ReadStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncDisposable SubscribeToAllStreams(SubscribeToAllStreamsRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public IAsyncDisposable SubscribeToStream(SubscribeToStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    public Task TruncateStreamAsync(TruncateStreamRequest request, CancellationToken cancellationToken = default) => throw new NotImplementedException();
}
