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
using System.Net.Http.Headers;
using System.Rests;
using System.Rests.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestRequestBuilderTests
{
    [Fact]
    public async Task BuildRequestAsync_WithContext_ExecutesComposers()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        var fakeComposer = new FakeRequestComposer();

        RestRequestBuilder builder = new([fakeComposer]);

        SimpleRequest request = new();
        RestAttribute attribute = request.Build(null!);

        HttpRequestMessage httpMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("/test", UriKind.Relative)
        };

        RestRequestContext context = new()
        {
            Attribute = attribute,
            Request = request,
            Message = httpMessage,
            SerializerOptions = RestSettings.SerializerOptions,
            IsAborted = false
        };

        // Act
        RestRequest restRequest = await builder.BuildRequestAsync(context);

        // Assert
        using (restRequest)
        {
            fakeComposer.WasInvoked.Should().BeTrue();
        }
    }

    [Fact]
    public async Task BuildRequestAsync_WhenInterceptorAborts_ReturnsEmptyRequest()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        var interceptor = new AbortingRequestInterceptor();

        IServiceProvider services = new ServiceCollection()
            .AddXRestRequestComposers()
            .AddSingleton<IRestRequestInterceptor>(interceptor)
            .BuildServiceProvider();

        RestRequestBuilder builder = new(
            services.GetServices<IRestRequestComposer>(),
            requestInterceptors: services.GetServices<IRestRequestInterceptor>());

        SimpleRequest request = new();
        RestAttribute attribute = request.Build(services);

        HttpRequestMessage httpMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("/test", UriKind.Relative)
        };

        RestRequestContext context = new()
        {
            Attribute = attribute,
            Request = request,
            Message = httpMessage,
            SerializerOptions = RestSettings.SerializerOptions,
            IsAborted = false
        };

        // Act
        RestRequest restRequest = await builder.BuildRequestAsync(context);

        // Assert
        interceptor.WasInvoked.Should().BeTrue();
        context.IsAborted.Should().BeTrue();
    }

    [Fact]
    public async Task BuildRequestAsync_NoComposerFound_Throws()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange - no composers registered
        IServiceProvider services = new ServiceCollection()
            .BuildServiceProvider();

        RestRequestBuilder builder = new(services.GetServices<IRestRequestComposer>());

        SimpleRequest request = new();
        RestAttribute attribute = request.Build(services);

        HttpRequestMessage httpMessage = new()
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri("/test", UriKind.Relative)
        };

        RestRequestContext context = new()
        {
            Attribute = attribute,
            Request = request,
            Message = httpMessage,
            SerializerOptions = RestSettings.SerializerOptions,
            IsAborted = false
        };

        // Act
        Func<Task> act = async () => await builder.BuildRequestAsync(context);

        // Assert
        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*No request builder found*");
    }

    private static IDisposable UseDefaultSerializerOptions()
    {
        JsonSerializerOptions previous = RestSettings.SerializerOptions;
        RestSettings.SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };

        return new DelegateDisposable(() => RestSettings.SerializerOptions = previous);
    }

    private sealed class DelegateDisposable(Action dispose) : IDisposable
    {
        private readonly Action _dispose = dispose;
        public void Dispose() => _dispose();
    }

    private sealed record SimpleRequest : IRestRequest, IRestAttributeBuilder
    {
        public RestAttribute Build(IServiceProvider serviceProvider) =>
            new RestGetAttribute("/test");
    }

    private sealed record PlaceOrderRequest(string OrderId, string Product, int Quantity)
        : IRestRequest, IRestString, IRestPathString, IRestAttributeBuilder
    {
        public IDictionary<string, string> GetPathString() => new Dictionary<string, string>
        {
            ["orderId"] = OrderId
        };

        public RestAttribute Build(IServiceProvider serviceProvider) => new RestPostAttribute("/orders/{orderId}")
        {
            Location = RestSettings.Location.Body | RestSettings.Location.Path,
            BodyFormat = RestSettings.BodyFormat.String,
            ContentType = RestSettings.ContentType.Json,
            Accept = RestSettings.ContentType.Json
        };
    }

        private sealed class AbortingRequestInterceptor : IRestRequestInterceptor
        {
            public bool WasInvoked { get; private set; }

            public ValueTask InterceptAsync(RestRequestContext context, CancellationToken cancellationToken = default)
            {
                WasInvoked = true;
                context.IsAborted = true;
                return ValueTask.CompletedTask;
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
    }
