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
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Events.Domain;
using System.Runtime.CompilerServices;

namespace System.Events.Data;

/// <summary>
/// Provides an ADO.NET implementation of a domain store that persists domain events and snapshots.
/// </summary>
/// <remarks>
/// <para>
/// This class enables appending, reading, and managing event streams and snapshots in an event-sourced
/// system using raw ADO.NET (not Entity Framework Core).
/// </para>
/// <para>
/// All operations are asynchronous and support cancellation via CancellationToken.
/// This class is not thread-safe; concurrent usage should be managed externally if required.
/// </para>
/// </remarks>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
public sealed class DomainStore<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventDomain,
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TDataEventSnapshot> : IDomainStore
	where TDataEventDomain : class, IDataEventDomain
	where TDataEventSnapshot : class, IDataEventSnapshot
{
	private readonly IDataRepository<TDataEventDomain> _domainRepository;
	private readonly IDataRepository<TDataEventSnapshot> _snapshotRepository;
	private readonly IEventConverter<TDataEventDomain, IDomainEvent> _domainConverter;
	private readonly IEventConverter<TDataEventSnapshot, ISnapshotEvent> _snapshotConverter;

	/// <summary>
	/// Initializes a new instance of the DomainStore class.
	/// </summary>
	/// <param name="unitOfWork">The ADO.NET unit of work used to manage repositories and transactions.</param>
	/// <param name="converterProvider">The provider used to obtain event converters for domain and snapshot events.</param>
	public DomainStore(IDataUnitOfWork unitOfWork, IEventConverterProvider converterProvider)
	{
		ArgumentNullException.ThrowIfNull(unitOfWork);
		ArgumentNullException.ThrowIfNull(converterProvider);

		_domainRepository = unitOfWork.GetRepository<TDataEventDomain>();
		_snapshotRepository = unitOfWork.GetRepository<TDataEventSnapshot>();

		_domainConverter = converterProvider.GetEventConverter<TDataEventDomain, IDomainEvent>();
		_snapshotConverter = converterProvider.GetEventConverter<TDataEventSnapshot, ISnapshotEvent>();
	}

	/// <inheritdoc/>
	public async Task<AppendResult> AppendToStreamAsync(
		AppendRequest request,
		CancellationToken cancellationToken = default)
	{
		IDomainEvent[] batch = [.. request.Events.OfType<IDomainEvent>()];
		if (batch.Length == 0)
		{
			return AppendResult.Create(
				request.ExpectedVersion.GetValueOrDefault() + 1,
				request.ExpectedVersion.GetValueOrDefault());
		}

		long current = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken).ConfigureAwait(false);
		if (request.ExpectedVersion.HasValue && current != request.ExpectedVersion)
		{
			throw new InvalidOperationException(
				$"Concurrency violation for aggregate {request.StreamId}. " +
				$"Expected version {request.ExpectedVersion} but found {current}.");
		}

		var entities = new List<TDataEventDomain>(capacity: batch.Length);
		long next = request.ExpectedVersion.GetValueOrDefault();

		foreach (IDomainEvent? @event in batch)
		{
			next++;

			IDomainEvent nextEvent = @event
				.WithStreamId(request.StreamId)
				.WithStreamVersion(next)
				.WithStreamName(@event.StreamName);

			TDataEventDomain entity = _domainConverter.ConvertEventToData(nextEvent);
			entities.Add(entity);
		}

		await _domainRepository.InsertAsync(entities, cancellationToken).ConfigureAwait(false);

		Guid[] guids = [.. entities.ConvertAll(e => e.StreamId)];
		return AppendResult.Create(guids, next, request.ExpectedVersion.GetValueOrDefault());
	}

	/// <inheritdoc/>
	public async Task AppendSnapshotAsync(
		ISnapshotEvent @event,
		CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(@event);

		TDataEventSnapshot entity = _snapshotConverter.ConvertEventToData(@event);
		await _snapshotRepository.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task DeleteStreamAsync(
		DeleteStreamRequest request,
		CancellationToken cancellationToken = default)
	{
		DataSpecification<TDataEventDomain, TDataEventDomain> specification = DataSpecification
			.For<TDataEventDomain>()
			.Where(e => e.StreamId == request.StreamId)
			.Build();

		if (request.HardDelete)
		{
			await _domainRepository
				.DeleteAsync(specification, cancellationToken)
				.ConfigureAwait(false);
		}
		else
		{
			DataUpdater<TDataEventDomain> updater = DataUpdater
				.For<TDataEventDomain>()
				.SetProperty(e => e.Status, EventStatus.DELETED.Value);

			await _domainRepository
				.UpdateAsync(specification, updater, cancellationToken)
				.ConfigureAwait(false);
		}
	}

	/// <inheritdoc/>
	public async Task<EnvelopeResult?> GetLatestSnapshotAsync(
		Guid ownerId,
		CancellationToken cancellationToken = default)
	{
		DataSpecification<TDataEventSnapshot, TDataEventSnapshot> specification = DataSpecification
			.For<TDataEventSnapshot>()
			.Where(e => e.OwnerId == ownerId)
			.OrderByDescending(e => e.Sequence)
			.Take(1)
			.Build();

		TDataEventSnapshot? last = await _snapshotRepository
			.QueryFirstOrDefaultAsync(specification, cancellationToken)
			.ConfigureAwait(false);

		if (last == null)
		{
			return null;
		}

		return new EnvelopeResult
		{
			Event = _snapshotConverter.ConvertDataToEvent(last),
			EventId = last.KeyId,
			EventName = last.EventName,
			GlobalPosition = last.Sequence,
			OccurredOn = last.CreatedOn,
			StreamId = last.OwnerId,
			StreamName = null,
			StreamVersion = last.Sequence
		};
	}

	/// <inheritdoc/>
	public async Task<long> GetStreamVersionAsync(Guid streamId, CancellationToken cancellationToken = default) =>
		await GetStreamVersionCoreAsync(streamId, cancellationToken).ConfigureAwait(false);

	/// <inheritdoc/>
	public async IAsyncEnumerable<EnvelopeResult> ReadAllStreamsAsync(
		ReadAllStreamsRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		DataSpecification<TDataEventDomain, TDataEventDomain> specification = DataSpecification
			.For<TDataEventDomain>()
			.Where(e => e.Sequence > request.FromPosition)
			.OrderBy(e => e.Sequence)
			.Take(request.MaxCount)
			.Build();

		await foreach (TDataEventDomain? entity in _domainRepository.QueryAsync(specification, cancellationToken).ConfigureAwait(false))
		{
			yield return new EnvelopeResult
			{
				Event = _domainConverter.ConvertDataToEvent(entity),
				EventId = entity.KeyId,
				EventName = entity.EventName,
				GlobalPosition = entity.Sequence,
				OccurredOn = entity.CreatedOn,
				StreamId = entity.StreamId,
				StreamName = entity.StreamName,
				StreamVersion = entity.StreamVersion,
				CausationId = entity.CausationId,
				CorrelationId = entity.CorrelationId
			};
		}
	}

	/// <inheritdoc/>
	public async IAsyncEnumerable<EnvelopeResult> ReadStreamAsync(
		ReadStreamRequest request,
		[EnumeratorCancellation] CancellationToken cancellationToken = default)
	{
		DataSpecification<TDataEventDomain, TDataEventDomain> specification = DataSpecification
			.For<TDataEventDomain>()
			.Where(e => e.StreamId == request.StreamId && e.StreamVersion > request.FromVersion)
			.OrderBy(e => e.StreamVersion)
			.Take(request.MaxCount)
			.Build();

		await foreach (TDataEventDomain? entity in _domainRepository.QueryAsync(specification, cancellationToken).ConfigureAwait(false))
		{
			yield return new EnvelopeResult
			{
				Event = _domainConverter.ConvertDataToEvent(entity),
				EventId = entity.KeyId,
				EventName = entity.EventName,
				GlobalPosition = entity.Sequence,
				OccurredOn = entity.CreatedOn,
				StreamId = entity.StreamId,
				StreamName = entity.StreamName,
				StreamVersion = entity.StreamVersion,
				CausationId = entity.CausationId,
				CorrelationId = entity.CorrelationId
			};
		}
	}

	/// <inheritdoc/>
	public async Task<bool> StreamExistsAsync(Guid streamId, CancellationToken cancellationToken = default)
	{
		DataSpecification<TDataEventDomain, TDataEventDomain> specification = DataSpecification
			.For<TDataEventDomain>()
			.Where(e => e.StreamId == streamId)
			.Build();

		return await _domainRepository
			.ExistsAsync(specification, cancellationToken)
			.ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public async Task TruncateStreamAsync(
		TruncateStreamRequest request,
		CancellationToken cancellationToken = default)
	{
		long currentVersion = await GetStreamVersionCoreAsync(request.StreamId, cancellationToken)
			.ConfigureAwait(false);

		if (currentVersion == -1)
		{
			return;
		}

		DataSpecification<TDataEventDomain, TDataEventDomain> specification = DataSpecification
			.For<TDataEventDomain>()
			.Where(e => e.StreamId == request.StreamId && e.StreamVersion < request.TruncateBeforeVersion)
			.Build();

		await _domainRepository
			.DeleteAsync(specification, cancellationToken)
			.ConfigureAwait(false);
	}

	/// <inheritdoc/>
	public IAsyncDisposable SubscribeToStream(
		SubscribeToStreamRequest request,
		CancellationToken cancellationToken = default)
	{
		// ADO.NET doesn't support change notifications natively
		// Return a polling-based subscription
		return new StreamSubscription<TDataEventDomain>(
			_domainRepository,
			request,
			_domainConverter,
			cancellationToken);
	}

	/// <inheritdoc/>
	public IAsyncDisposable SubscribeToAllStreams(
		SubscribeToAllStreamsRequest request,
		CancellationToken cancellationToken = default)
	{
		return new AllStreamsSubscription<TDataEventDomain>(
			_domainRepository,
			request,
			_domainConverter,
			cancellationToken);
	}

	private async Task<long> GetStreamVersionCoreAsync(Guid streamId, CancellationToken cancellationToken)
	{
		DataSpecification<TDataEventDomain, TDataEventDomain> specification = DataSpecification
			.For<TDataEventDomain>()
			.Where(e => e.StreamId == streamId)
			.OrderByDescending(e => e.StreamVersion)
			.Build();

		TDataEventDomain? @event = await _domainRepository
			.QueryFirstOrDefaultAsync(specification, cancellationToken)
			.ConfigureAwait(false);

		return @event?.StreamVersion ?? -1;
	}
}

/// <summary>
/// Polling-based stream subscription for ADO.NET.
/// </summary>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class StreamSubscription<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventDomain>
	: IAsyncDisposable
	where TEntityEventDomain : class, IDataEventDomain
#pragma warning restore CA1812
{
	private readonly IDataRepository<TEntityEventDomain> _domainRepository;
	private readonly SubscribeToStreamRequest _request;
	private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
	private readonly CancellationTokenSource _cts;
	private readonly Task _subscriptionTask;

	public StreamSubscription(
		IDataRepository<TEntityEventDomain> repository,
		SubscribeToStreamRequest request,
		IEventConverter<TEntityEventDomain, IDomainEvent> converter,
		CancellationToken cancellationToken)
	{
		_domainRepository = repository ?? throw new ArgumentNullException(nameof(repository));
		_request = request;
		_domainConverter = converter ?? throw new ArgumentNullException(nameof(converter));
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_subscriptionTask = RunSubscriptionAsync();
	}

	private async Task RunSubscriptionAsync()
	{
		long lastProcessedVersion = _request.FromVersion - 1;

		try
		{
			while (!_cts.Token.IsCancellationRequested)
			{
				DataSpecification<TEntityEventDomain, TEntityEventDomain> specification = DataSpecification
					.For<TEntityEventDomain>()
					.Where(e => e.StreamId == _request.StreamId && e.StreamVersion > lastProcessedVersion)
					.OrderBy(e => e.StreamVersion)
					.Take(100)
					.Build();

				var events = new List<TEntityEventDomain>();
				await foreach (TEntityEventDomain? entity in _domainRepository.QueryAsync(specification, _cts.Token).ConfigureAwait(false))
				{
					events.Add(entity);
				}

				foreach (TEntityEventDomain entity in events)
				{
					IDomainEvent domainEvent = _domainConverter.ConvertDataToEvent(entity);
					await _request.OnEvent(domainEvent).ConfigureAwait(false);
					lastProcessedVersion = entity.StreamVersion;
				}

				if (events.Count == 0)
				{
					await Task.Delay(_request.PollingInterval, _cts.Token).ConfigureAwait(false);
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when disposed
		}
	}

	public async ValueTask DisposeAsync()
	{
		await _cts.CancelAsync().ConfigureAwait(false);
		try
		{
			await _subscriptionTask.ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			// Expected
		}
		_cts.Dispose();
	}
}

/// <summary>
/// Polling-based all-streams subscription for ADO.NET.
/// </summary>
[RequiresDynamicCode("Expression compilation requires dynamic code generation.")]
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
internal sealed class AllStreamsSubscription<
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)] TEntityEventDomain>
	: IAsyncDisposable
	where TEntityEventDomain : class, IDataEventDomain
#pragma warning restore CA1812
{
	private readonly IDataRepository<TEntityEventDomain> _domainRepository;
	private readonly SubscribeToAllStreamsRequest _request;
	private readonly IEventConverter<TEntityEventDomain, IDomainEvent> _domainConverter;
	private readonly CancellationTokenSource _cts;
	private readonly Task _subscriptionTask;

	public AllStreamsSubscription(
		IDataRepository<TEntityEventDomain> repository,
		SubscribeToAllStreamsRequest request,
		IEventConverter<TEntityEventDomain, IDomainEvent> converter,
		CancellationToken cancellationToken)
	{
		_domainRepository = repository ?? throw new ArgumentNullException(nameof(repository));
		_request = request;
		_domainConverter = converter ?? throw new ArgumentNullException(nameof(converter));
		_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
		_subscriptionTask = RunSubscriptionAsync();
	}

	private async Task RunSubscriptionAsync()
	{
		long lastProcessedSequence = _request.FromPosition - 1;

		try
		{
			while (!_cts.Token.IsCancellationRequested)
			{
				DataSpecification<TEntityEventDomain, TEntityEventDomain> specification = DataSpecification
					.For<TEntityEventDomain>()
					.Where(e => e.Sequence > lastProcessedSequence)
					.OrderBy(e => e.Sequence)
					.Take(100)
					.Build();

				var events = new List<TEntityEventDomain>();
				await foreach (TEntityEventDomain? entity in _domainRepository.QueryAsync(specification, _cts.Token).ConfigureAwait(false))
				{
					events.Add(entity);
				}

				foreach (TEntityEventDomain entity in events)
				{
					IDomainEvent domainEvent = _domainConverter.ConvertDataToEvent(entity);
					await _request.OnEvent(domainEvent).ConfigureAwait(false);
					lastProcessedSequence = entity.Sequence;
				}

				if (events.Count == 0)
				{
					await Task.Delay(_request.PollingInterval, _cts.Token).ConfigureAwait(false);
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Expected when disposed
		}
	}

	public async ValueTask DisposeAsync()
	{
		await _cts.CancelAsync().ConfigureAwait(false);
		try
		{
			await _subscriptionTask.ConfigureAwait(false);
		}
		catch (OperationCanceledException)
		{
			// Expected
		}
		_cts.Dispose();
	}
}
