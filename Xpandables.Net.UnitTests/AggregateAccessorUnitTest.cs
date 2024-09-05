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
namespace Xpandables.Net.UnitTests;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using Moq;

using Xpandables.Net.Aggregates;
using Xpandables.Net.Distribution;
using Xpandables.Net.Events;
using Xpandables.Net.Operations;
using Xpandables.Net.Primitives.Collections;

using Xunit;

public class AggregateAccessorTests
{
    private readonly Mock<IEventStore> _eventStoreMock;
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly Mock<IOptions<EventOptions>> _optionsMock;
    private readonly AggregateAccessor<TestAggregate> _aggregateAccessor;

    public AggregateAccessorTests()
    {
        EventOptions eventOptions = new();
        EventOptions.Default(eventOptions);

        _eventStoreMock = new Mock<IEventStore>();
        _publisherMock = new Mock<IEventPublisher>();
        _optionsMock = new Mock<IOptions<EventOptions>>();
        _optionsMock.Setup(o => o.Value).Returns(eventOptions);

        _aggregateAccessor = new AggregateAccessor<TestAggregate>(
            _eventStoreMock.Object,
            _publisherMock.Object,
            _optionsMock.Object);
    }

    [Fact]
    public async Task AppendAsync_ShouldReturnOkResult_WhenEventsAreAppendedAndPublishedSuccessfully()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.AddEvent(new TestEvent());

        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<IEventDomain>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResults.Ok().Build());

        // Act
        var result = await _aggregateAccessor.AppendAsync(aggregate);

        // Assert
        Assert.True(result.IsSuccess);
        _eventStoreMock.Verify(es => es.AppendAsync(It.IsAny<IEventDomain>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<IEventDomain>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AppendAsync_ShouldReturnFailureResult_WhenEventPublishingFails()
    {
        // Arrange
        var aggregate = new TestAggregate();
        aggregate.AddEvent(new TestEvent());

        _publisherMock
            .Setup(p => p.PublishAsync(It.IsAny<IEventDomain>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(OperationResults.InternalError().Build());

        // Act
        var result = await _aggregateAccessor.AppendAsync(aggregate);

        // Assert
        Assert.True(result.IsFailure);
        _eventStoreMock.Verify(es => es.AppendAsync(It.IsAny<IEventDomain>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _publisherMock.Verify(p => p.PublishAsync(It.IsAny<IEventDomain>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PeekAsync_ShouldReturnOkResultWithAggregate_WhenEventsAreFetchedSuccessfully()
    {
        // Arrange
        var keyId = Guid.NewGuid();
        var aggregate = new TestAggregate();
        var testEvent = new TestEvent();

        _eventStoreMock
            .Setup(es => es.FetchAsync(It.IsAny<IEventFilter>(),
            It.IsAny<CancellationToken>()))
            .Returns(new[] { testEvent }.ToAsyncEnumerable());

        // Act
        var result = await _aggregateAccessor.PeekAsync(keyId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Result);
        _eventStoreMock.Verify(es => es.FetchAsync(It.IsAny<IEventFilter>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PeekAsync_ShouldReturnNotFoundResult_WhenNoEventsAreFetched()
    {
        // Arrange
        var keyId = Guid.NewGuid();

        _eventStoreMock
            .Setup(es => es.FetchAsync(It.IsAny<IEventFilter>(),
            It.IsAny<CancellationToken>()))
            .Returns(AsyncEnumerable.Empty<IEvent>());

        // Act
        var result = await _aggregateAccessor.PeekAsync(keyId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(System.Net.HttpStatusCode.NotFound, result.StatusCode);
        _eventStoreMock.Verify(es => es.FetchAsync(It.IsAny<IEventFilter>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private class TestAggregate : Aggregate
    {
        private readonly List<IEventDomain> _uncommittedEvents = [];

        public bool IsEmpty => _uncommittedEvents.Count == 0;

        public void AddEvent(IEventDomain @event) => _uncommittedEvents.Add(@event);

        public override IReadOnlyCollection<IEventDomain> GetUncommittedEvents()
            => _uncommittedEvents;

        public override void LoadFromHistory(IEventDomain @event)
        {
            _uncommittedEvents.Add(@event);
            AggregateId = @event.AggregateId;
        }

        public override void MarkEventsAsCommitted() => _uncommittedEvents.Clear();
    }

    private sealed record TestEvent : EventDomain
    {
        [SetsRequiredMembers]
        public TestEvent() => AggregateId = Guid.NewGuid();
    }
}
