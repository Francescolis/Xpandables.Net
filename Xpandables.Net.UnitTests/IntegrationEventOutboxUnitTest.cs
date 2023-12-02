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
using System.Net;

using Moq;

using Xpandables.Net.IntegrationEvents;

namespace Xpandables.Net.UnitTests;
public sealed class IntegrationEventOutboxUnitTest
{
    private readonly Mock<IIntegrationEventSourcing> _eventSourcingMock = new();
    private readonly Mock<IIntegrationEventStore> _eventStoreMock = new();

    [Fact]
    public async Task AppendAsync_Should_Return_OperationResult_Ok_When_No_IntegrationEvent()
    {
        // Arrange
        _eventSourcingMock.Setup(x => x.GetIntegrationEvents())
            .Returns(Array.Empty<IIntegrationEvent>().OrderBy(o => o.Id));

        var outbox = new IntegrationEventOutbox(
            _eventSourcingMock.Object, _eventStoreMock.Object);

        // Act
        var result = await outbox.AppendAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }

    [Fact]
    public async Task AppendAsync_Should_Return_OperationResult_InternalError_When_Exception_Occurs()
    {
        // Arrange
        _eventSourcingMock.Setup(x => x.GetIntegrationEvents())
            .Returns(new[] { new Mock<IIntegrationEvent>().Object }.OrderBy(o => o.Id));
        _eventStoreMock.Setup(x => x.AppendAsync(It.IsAny<IIntegrationEvent>(), default))
            .ThrowsAsync(new Exception());

        var outbox = new IntegrationEventOutbox(_eventSourcingMock.Object, _eventStoreMock.Object);

        // Act
        var result = await outbox.AppendAsync();

        // Assert
        Assert.Equal(HttpStatusCode.InternalServerError, result.StatusCode);
    }

    [Fact]
    public async Task AppendAsync_Should_Return_OperationResult_Ok_When_Append_Successful()
    {
        // Arrange
        _eventSourcingMock.Setup(x => x.GetIntegrationEvents())
            .Returns(new[] { new Mock<IIntegrationEvent>().Object }.OrderBy(o => o.Id));

        var outbox = new IntegrationEventOutbox(_eventSourcingMock.Object, _eventStoreMock.Object);

        // Act
        var result = await outbox.AppendAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
    }
}