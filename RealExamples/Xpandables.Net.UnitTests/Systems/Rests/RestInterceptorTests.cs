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
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestInterceptorTests
{
    #region Request Interceptor Tests

    [Fact]
    public async Task RequestInterceptor_IsInvoked_DuringBuildRequest()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        var interceptor = new TestRequestInterceptor(abort: false);
        var fakeComposer = new FakeRequestComposer();

        RestRequestBuilder builder = new(
            [fakeComposer],
            requestInterceptors: [interceptor]);

        RestRequestContext context = CreateRequestContext();

        // Act
        await builder.BuildRequestAsync(context);

        // Assert
        interceptor.WasInvoked.Should().BeTrue();
        fakeComposer.WasInvoked.Should().BeTrue("Composer should be called when not aborted");
    }

    [Fact]
    public async Task RequestInterceptor_WhenAborted_SkipsComposers()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        var interceptor = new TestRequestInterceptor(abort: true);
        var fakeComposer = new FakeRequestComposer();

        RestRequestBuilder builder = new(
            [fakeComposer],
            requestInterceptors: [interceptor]);

        RestRequestContext context = CreateRequestContext();

        // Act
        RestRequest result = await builder.BuildRequestAsync(context);

        // Assert
        interceptor.WasInvoked.Should().BeTrue();
        context.IsAborted.Should().BeTrue();
        fakeComposer.WasInvoked.Should().BeFalse("Composer should be skipped when aborted");
    }

    [Fact]
    public async Task RequestInterceptors_ExecuteInOrder()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        List<int> executionOrder = [];
        var interceptor1 = new OrderedRequestInterceptor(1, executionOrder, order: 10);
        var interceptor2 = new OrderedRequestInterceptor(2, executionOrder, order: 5);
        var interceptor3 = new OrderedRequestInterceptor(3, executionOrder, order: 15);
        var fakeComposer = new FakeRequestComposer();

        RestRequestBuilder builder = new(
            [fakeComposer],
            requestInterceptors: [interceptor1, interceptor2, interceptor3]);

        RestRequestContext context = CreateRequestContext();

        // Act
        await builder.BuildRequestAsync(context);

        // Assert - should execute in order: 5, 10, 15 (by Order property)
        executionOrder.Should().ContainInOrder(2, 1, 3);
    }

    #endregion

    #region Response Interceptor Tests

    [Fact]
    public async Task ResponseInterceptor_IsInvoked_DuringBuildResponse()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        var interceptor = new TestResponseInterceptor();
        var fakeComposer = new FakeResponseComposer();

        RestResponseBuilder builder = new(
            [fakeComposer],
            responseInterceptors: [interceptor]);

        RestResponseContext context = CreateResponseContext();

        // Act
        await builder.BuildResponseAsync(context);

        // Assert
        interceptor.WasInvoked.Should().BeTrue();
        interceptor.ReceivedContext.Should().BeSameAs(context);
    }

    [Fact]
    public async Task ResponseInterceptors_ExecuteInOrder()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        List<int> executionOrder = [];
        var interceptor1 = new OrderedResponseInterceptor(1, executionOrder, order: 20);
        var interceptor2 = new OrderedResponseInterceptor(2, executionOrder, order: 1);
        var interceptor3 = new OrderedResponseInterceptor(3, executionOrder, order: 10);
        var fakeComposer = new FakeResponseComposer();

        RestResponseBuilder builder = new(
            [fakeComposer],
            responseInterceptors: [interceptor1, interceptor2, interceptor3]);

        RestResponseContext context = CreateResponseContext();

        // Act
        await builder.BuildResponseAsync(context);

        // Assert - should execute in order: 1, 10, 20 (by Order property)
        executionOrder.Should().ContainInOrder(2, 3, 1);
    }

    [Fact]
    public async Task ResponseInterceptor_CanModifyResponse()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        RestResponse modifiedResponse = new()
        {
            StatusCode = HttpStatusCode.Created,
            Headers = ElementCollection.Empty,
            Version = HttpVersion.Version20
        };
        var interceptor = new ModifyingResponseInterceptor(modifiedResponse);
        var fakeComposer = new FakeResponseComposer();

        RestResponseBuilder builder = new(
            [fakeComposer],
            responseInterceptors: [interceptor]);

        RestResponseContext context = CreateResponseContext();

        // Act
        RestResponse result = await builder.BuildResponseAsync(context);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    #endregion

    #region Helper Methods

    private static IDisposable UseDefaultSerializerOptions()
    {
        JsonSerializerOptions previous = RestSettings.SerializerOptions;
        RestSettings.SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        return new DelegateDisposable(() => RestSettings.SerializerOptions = previous);
    }

    private static RestRequestContext CreateRequestContext()
    {
        return new RestRequestContext
        {
            Attribute = new RestGetAttribute("/test"),
            Request = new SimpleRequest("test"),
            Message = new HttpRequestMessage(HttpMethod.Get, "/test"),
            SerializerOptions = RestSettings.SerializerOptions,
            IsAborted = false
        };
    }

    private static RestResponseContext CreateResponseContext()
    {
        return new RestResponseContext
        {
            Request = new SimpleRequest("test"),
            Message = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            },
            SerializerOptions = RestSettings.SerializerOptions,
            IsAborted = false
        };
    }

    #endregion

    #region Test Helpers

    private sealed class DelegateDisposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }

    private sealed record SimpleRequest(string Data) : IRestRequest, IRestAttributeBuilder
    {
        public RestAttribute Build(IServiceProvider serviceProvider) =>
            new RestGetAttribute("/test");
    }

    private sealed class TestRequestInterceptor(bool abort) : IRestRequestInterceptor
    {
        public bool WasInvoked { get; private set; }

        public ValueTask InterceptAsync(RestRequestContext context, CancellationToken cancellationToken = default)
        {
            WasInvoked = true;

            if (abort)
            {
                context.IsAborted = true;
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class OrderedRequestInterceptor(int id, List<int> executionOrder, int order)
        : IRestRequestInterceptor
    {
        public int Order => order;

        public ValueTask InterceptAsync(RestRequestContext context, CancellationToken cancellationToken = default)
        {
            executionOrder.Add(id);
            return ValueTask.CompletedTask;
        }
    }

    private sealed class TestResponseInterceptor : IRestResponseInterceptor
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

    private sealed class OrderedResponseInterceptor(int id, List<int> executionOrder, int order)
        : IRestResponseInterceptor
    {
        public int Order => order;

        public ValueTask<RestResponse> InterceptAsync(
            RestResponseContext context,
            RestResponse response,
            CancellationToken cancellationToken = default)
        {
            executionOrder.Add(id);
            return ValueTask.FromResult(response);
        }
    }

    private sealed class ModifyingResponseInterceptor(RestResponse modifiedResponse) : IRestResponseInterceptor
    {
        public ValueTask<RestResponse> InterceptAsync(
            RestResponseContext context,
            RestResponse response,
            CancellationToken cancellationToken = default)
        {
            return ValueTask.FromResult(modifiedResponse);
        }
    }

    private sealed class FakeRequestComposer : IRestRequestComposer
    {
        public bool WasInvoked { get; private set; }

        public bool CanCompose(RestRequestContext context) => true;

        public ValueTask ComposeAsync(RestRequestContext context, CancellationToken cancellationToken = default)
        {
            WasInvoked = true;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeResponseComposer : IRestResponseComposer
    {
        public bool WasInvoked { get; private set; }

        public bool CanCompose(RestResponseContext context) => true;

        public ValueTask<RestResponse> ComposeAsync(RestResponseContext context, CancellationToken cancellationToken = default)
        {
            WasInvoked = true;
            return ValueTask.FromResult(new RestResponse
            {
                StatusCode = HttpStatusCode.OK,
                Headers = ElementCollection.Empty,
                Version = HttpVersion.Version20
            });
        }
    }

    #endregion
}
