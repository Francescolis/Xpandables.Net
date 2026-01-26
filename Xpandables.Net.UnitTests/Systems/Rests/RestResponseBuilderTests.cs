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
using System.Collections;
using System.Net;
using System.Rests;
using System.Rests.Abstractions;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestResponseBuilderTests
{
    [Fact]
    public async Task BuildResponseAsync_UsesFirstMatchingComposer()
    {
        // Arrange
        RestResponse expectedResponse = new()
        {
            StatusCode = HttpStatusCode.OK,
            Headers = ElementCollection.Empty,
            Version = HttpVersion.Version20
        };

        FakeComposer nonMatchingComposer = new(canCompose: false, new RestResponse
        {
            StatusCode = HttpStatusCode.InternalServerError,
            Headers = ElementCollection.Empty,
            Version = HttpVersion.Version11
        });

        FakeComposer matchingComposer = new(canCompose: true, expectedResponse);

        IServiceProvider services = new ServiceCollection()
            .AddSingleton<IRestResponseComposer>(nonMatchingComposer)
            .AddSingleton<IRestResponseComposer>(matchingComposer)
            .BuildServiceProvider();

        RestResponseBuilder builder = new(services.GetServices<IRestResponseComposer>());
        RestResponseContext context = CreateContext();

        // Act
        RestResponse response = await builder.BuildResponseAsync(context);

        // Assert
        response.Should().BeSameAs(expectedResponse);
        matchingComposer.WasInvoked.Should().BeTrue();
        nonMatchingComposer.WasInvoked.Should().BeFalse();
    }

    [Fact]
    public async Task BuildResponseAsync_NoComposerFound_Throws()
    {
        // Arrange
        RestResponseBuilder builder =
            new(new ServiceCollection().BuildServiceProvider().GetServices<IRestResponseComposer>());
        RestResponseContext context = CreateContext();

        // Act
        Func<Task> act = async () => await builder.BuildResponseAsync(context);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*No composer found*");
    }

    [Fact]
    public async Task BuildResponseAsync_ExecutesResponseInterceptors()
    {
        // Arrange
        RestResponse expectedResponse = new()
        {
            StatusCode = HttpStatusCode.OK,
            Headers = ElementCollection.Empty,
            Version = HttpVersion.Version20
        };

        FakeComposer composer = new(canCompose: true, expectedResponse);
        FakeResponseInterceptor interceptor = new();

        IServiceProvider services = new ServiceCollection()
            .AddSingleton<IRestResponseComposer>(composer)
            .AddSingleton<IRestResponseInterceptor>(interceptor)
            .BuildServiceProvider();

        RestResponseBuilder builder = new(
            services.GetServices<IRestResponseComposer>(),
            responseInterceptors: services.GetServices<IRestResponseInterceptor>());

        RestResponseContext context = CreateContext();

        // Act
        await builder.BuildResponseAsync(context);

        // Assert
        interceptor.WasInvoked.Should().BeTrue();
        interceptor.ReceivedContext.Should().BeSameAs(context);
    }

    [Fact]
    public async Task BuildResponseAsync_WhenAborted_ReturnsEmptyResponseWithoutCallingInterceptors()
    {
        // Arrange
        RestResponse expectedResponse = new()
        {
            StatusCode = HttpStatusCode.OK,
            Headers = ElementCollection.Empty,
            Version = HttpVersion.Version20
        };

        FakeComposer composer = new(canCompose: true, expectedResponse);
        FakeResponseInterceptor interceptor = new();

        IServiceProvider services = new ServiceCollection()
            .AddSingleton<IRestResponseComposer>(composer)
            .AddSingleton<IRestResponseInterceptor>(interceptor)
            .BuildServiceProvider();

        RestResponseBuilder builder = new(
            services.GetServices<IRestResponseComposer>(),
            responseInterceptors: services.GetServices<IRestResponseInterceptor>());

        RestResponseContext context = CreateContext(isAborted: true);

        // Act
        RestResponse response = await builder.BuildResponseAsync(context);

        // Assert - when aborted, returns empty and skips everything
        composer.WasInvoked.Should().BeFalse("Composer should not be called for aborted requests");
        interceptor.WasInvoked.Should().BeFalse("Interceptors should not be called for aborted requests");
    }

    private static RestResponseContext CreateContext(
        HttpStatusCode statusCode = HttpStatusCode.OK,
        bool isAborted = false) => new()
        {
            Request = new TestRequest(),
            Message = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(string.Empty)
            },
            SerializerOptions = RestSettings.SerializerOptions,
            IsAborted = isAborted
        };

    private sealed class TestRequest : IRestRequest;

    private sealed class FakeComposer(bool canCompose, RestResponse response) : IRestResponseComposer
    {
        public bool WasInvoked { get; private set; }

        public bool CanCompose(RestResponseContext context) => canCompose;

        public ValueTask<RestResponse> ComposeAsync(RestResponseContext context, CancellationToken cancellationToken)
        {
            WasInvoked = true;
            return ValueTask.FromResult(response);
        }
    }

    private sealed class FakeResponseInterceptor : IRestResponseInterceptor
    {
        public bool WasInvoked { get; private set; }
        public RestResponseContext? ReceivedContext { get; private set; }

        public ValueTask<RestResponse> InterceptAsync(
            RestResponseContext context,
            RestResponse response,
            CancellationToken cancellationToken = default)
        {
            WasInvoked = true;
            ReceivedContext = context;
            return ValueTask.FromResult(response);
        }
    }
}
