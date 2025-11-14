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
namespace Xpandables.Net.EventSourcing;

// Plan (pseudocode):
// - Core stream operations:
//   - AppendAsync(streamId, events, expectedVersion?, ct): append events with optimistic concurrency.
//   - ReadStreamAsync(streamId, fromVersion, ct): read forward from a stream.
//   - ReadAllAsync(fromPosition?, ct): read forward across all streams (if supported by store).
//   - GetStreamVersionAsync(streamId, ct): latest version of a stream.
//   - StreamExistsAsync(streamId, ct): does a stream exist.
// - Maintenance:
//   - DeleteStreamAsync(streamId, hardDelete, ct): soft/hard delete a stream.
//   - TruncateStreamAsync(streamId, toVersionInclusive, ct): remove older events up to a version.
// - Subscriptions (push-based):
//   - SubscribeToStream(streamId, fromVersion?, onEvent, ct) -> IAsyncDisposable: live stream subscription.
//   - SubscribeToAll(fromPosition?, onEvent, ct) -> IAsyncDisposable: live "all" subscription.
// - Notes:
//   - Use IAsyncEnumerable<object> for reads to support streaming.
//   - Use ValueTask in callbacks to minimize allocations.
//   - Use CancellationToken in all async operations.

/// <summary>
/// Defines the contract for an event store that supports appending, reading, deleting, truncating, and subscribing to
/// event streams in an asynchronous and scalable manner.
/// </summary>
/// <remarks>The event store interface provides core operations for working with event streams, including
/// optimistic concurrency control, streaming reads, and live event subscriptions. All operations are asynchronous and
/// support cancellation via a cancellation token. Implementations may support both per-stream and global (all-streams)
/// operations, as well as push-based subscriptions for real-time event processing. Thread safety and scalability
/// considerations depend on the specific implementation.</remarks>
public interface IEventStore : ISnapshotEventStore
{
    /// <summary>
    /// Appends events to a stream as specified by the provided <see cref="AppendRequest"/>.
    /// </summary>
    /// <param name="request">An <see cref="AppendRequest"/> object that specifies the data and options for the append operation. Cannot be
    /// null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the append operation.</param>
    /// <returns>A task that represents the asynchronous append operation. The task result contains an <see cref="AppendResult"/>
    /// describing the outcome of the operation.</returns>
    Task<AppendResult> AppendToStreamAsync(AppendRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously reads events from a stream according to the specified request parameters.
    /// </summary>
    /// <remarks>The returned sequence is evaluated lazily; events are read from the stream as the sequence is
    /// iterated. The caller is responsible for enumerating the sequence to initiate the read operation.</remarks>
    /// <param name="request">The request that specifies the stream to read from and the parameters controlling the read operation. Cannot be
    /// null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous read operation.</param>
    /// <returns>An asynchronous sequence of <see cref="EnvelopeResult"/> objects representing the events read from the stream.
    /// The sequence may be empty if no events are available.</returns>
    IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(ReadStreamRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously reads events from all streams according to the specified request parameters.
    /// </summary>
    /// <param name="request">The request that specifies the criteria for reading events from all streams. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>An asynchronous sequence of <see cref="EnvelopeResult"/> objects representing the events read from all streams.
    /// The sequence may be empty if no events match the request criteria.</returns>
    IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(ReadAllStreamsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously retrieves the current version number of the specified stream.
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream whose version is to be retrieved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the current version number of the
    /// stream, or -1 if the stream does not exist.</returns>
    Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously determines whether a stream with the specified identifier exists.
    /// </summary>
    /// <param name="streamId">The unique identifier of the stream to check for existence.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains <see langword="true"/> if the stream
    /// exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously deletes a stream as specified by the request parameters.
    /// </summary>
    /// <param name="request">The request containing the details of the stream to delete. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteStreamAsync(DeleteStreamRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously truncates a stream at the specified position, removing all events after that point.
    /// </summary>
    /// <remarks>Truncating a stream is an irreversible operation that permanently removes all events after
    /// the specified position. Use caution when calling this method, as truncated events cannot be recovered.</remarks>
    /// <param name="request">The request containing the stream identifier and the position at which to truncate. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is None.</param>
    /// <returns>A task that represents the asynchronous truncate operation.</returns>
    Task TruncateStreamAsync(TruncateStreamRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to a stream and returns an asynchronous disposable that manages the subscription lifecycle.
    /// </summary>
    /// <remarks>The subscription remains active until the returned <see cref="IAsyncDisposable"/> is disposed
    /// or the cancellation token is triggered. Multiple concurrent subscriptions to the same stream are supported
    /// unless otherwise specified by the implementation.</remarks>
    /// <param name="request">The subscription request specifying the stream to subscribe to and any additional options. Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the subscription operation.</param>
    /// <returns>An object that implements <see cref="IAsyncDisposable"/>. Disposing this object will terminate the subscription.</returns>
    IAsyncDisposable SubscribeToStream(SubscribeToStreamRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribes to all available streams and delivers events to the caller as they are received.
    /// </summary>
    /// <remarks>The returned subscription remains active until disposed or until the cancellation token is
    /// triggered. Events are delivered asynchronously as they arrive. Thread safety and event processing guarantees
    /// depend on the implementation of the subscription.</remarks>
    /// <param name="request">The subscription request specifying options for filtering, starting position, and event handling behavior.
    /// Cannot be null.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the subscription operation.</param>
    /// <returns>An <see cref="IAsyncDisposable"/> that can be disposed to terminate the subscription and release associated
    /// resources.</returns>
    IAsyncDisposable SubscribeToAllStreams(SubscribeToAllStreamsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously flushes all pending events to the underlying storage or destination.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the flush operation. The default value is <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous flush operation.</returns>
    /// <exception cref="InvalidOperationException">The process fails to flush the events.</exception>
    Task FlushEventsAsync(CancellationToken cancellationToken = default);
}
