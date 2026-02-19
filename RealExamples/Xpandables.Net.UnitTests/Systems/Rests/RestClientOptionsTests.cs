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
using System.Net;
using System.Rests.Abstractions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

namespace Xpandables.Net.UnitTests.Systems.Rests;

public sealed class RestClientOptionsTests
{
    [Fact]
    public void RestClientOptions_HasSensibleDefaults()
    {
        // Arrange & Act
        var options = new RestClientOptions();

        // Assert
        options.Timeout.Should().Be(TimeSpan.FromSeconds(30));
        options.Retry.Should().BeNull();
        options.CircuitBreaker.Should().BeNull();
        options.EnableLogging.Should().BeFalse();
        options.LogLevel.Should().Be(RestLogLevel.Information);
        options.LogRequestBody.Should().BeFalse();
        options.LogResponseBody.Should().BeFalse();
    }

    [Fact]
    public void RestRetryOptions_HasSensibleDefaults()
    {
        // Arrange & Act
        var options = new RestRetryOptions();

        // Assert
        options.MaxRetryAttempts.Should().Be(3);
        options.Delay.Should().Be(TimeSpan.FromSeconds(1));
        options.MaxDelay.Should().Be(TimeSpan.FromSeconds(30));
        options.UseExponentialBackoff.Should().BeTrue();
        options.JitterFactor.Should().Be(0.2);
        options.RetryableStatusCodes.Should().Contain(new[] { 408, 429, 500, 502, 503, 504 });
    }

    [Fact]
    public void RestCircuitBreakerOptions_HasSensibleDefaults()
    {
        // Arrange & Act
        var options = new RestCircuitBreakerOptions();

        // Assert
        options.FailureThreshold.Should().Be(5);
        options.BreakDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.SamplingDuration.Should().Be(TimeSpan.FromSeconds(30));
        options.MinimumThroughput.Should().Be(10);
    }

    // Note: RestClient timeout/cancellation tests require a properly configured request with matching composers
    // These tests are marked as skipped since they need full integration test setup
    // For proper testing, use the RestClientIntegrationTests class instead

    [Fact]
    public async Task RestClient_WhenRequestTimesOut_ReturnsTimeoutResponse()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        ServiceCollection services = new();
        services.AddXRestAttributeProvider();
        services.AddXRestRequestComposers();
        services.AddXRestResponseComposers();
        services.AddXRestRequestBuilder();
        services.AddXRestResponseBuilder();
        services.AddScoped<SlowHandler>();
        services.ConfigureXRestClientOptions(options =>
            options.Timeout = TimeSpan.FromMilliseconds(50));
        services.AddXRestClient((_, client) =>
            client.BaseAddress = new Uri("https://api.example.com"))
            .ConfigurePrimaryHttpMessageHandler<SlowHandler>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IRestClient client = serviceProvider.GetRequiredService<IRestClient>();

        // Act
        RestResponse response = await client.SendAsync(new SimpleRequest());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.RequestTimeout);
        response.Exception.Should().NotBeNull();
        response.Exception.Should().BeOfType<TimeoutException>();
    }

    [Fact]
    public async Task RestClient_WhenCancellationRequested_ReturnsCancelledState()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        ServiceCollection services = new();
        services.AddXRestAttributeProvider();
        services.AddXRestRequestComposers();
        services.AddXRestResponseComposers();
        services.AddXRestRequestBuilder();
        services.AddXRestResponseBuilder();
        services.AddScoped<SlowHandler>();
        services.AddXRestClient((_, client) =>
            client.BaseAddress = new Uri("https://api.example.com"))
            .ConfigurePrimaryHttpMessageHandler<SlowHandler>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IRestClient client = serviceProvider.GetRequiredService<IRestClient>();

        using CancellationTokenSource cts = new();
        cts.Cancel(); // Cancel immediately

		// Act & Assert - the response should indicate cancellation or throw
		// Since the SlowHandler will never be reached due to cancellation in builders
		RestResponse response = await client.SendAsync(new SimpleRequest(), cts.Token);
        response.Exception.Should().Match<Exception>(e =>
                   e is OperationCanceledException);
    }

    [Fact]
    public async Task RestClient_WhenHttpExceptionOccurs_ReturnsErrorResponse()
    {
        using IDisposable serializerScope = UseDefaultSerializerOptions();

        // Arrange
        ServiceCollection services = new();
        services.AddXRestAttributeProvider();
        services.AddXRestRequestComposers();
        services.AddXRestResponseComposers();
        services.AddXRestRequestBuilder();
        services.AddXRestResponseBuilder();
        services.AddScoped<FailingHandler>();
        services.AddXRestClient((_, client) =>
            client.BaseAddress = new Uri("https://api.example.com"))
            .ConfigurePrimaryHttpMessageHandler<FailingHandler>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        IRestClient client = serviceProvider.GetRequiredService<IRestClient>();

        // Act
        RestResponse response = await client.SendAsync(new SimpleRequest());

        // Assert
        response.IsSuccess.Should().BeFalse();
        response.Exception.Should().NotBeNull();
        // The HttpRequestException may be wrapped in an InvalidOperationException
        response.Exception.Should().Match<Exception>(e =>
            e is HttpRequestException || e.InnerException is HttpRequestException || e is TimeoutException);
    }

    [Fact]
    public void RestClientOptions_ConfigureOptions_AppliesSettings()
    {
        // Arrange
        ServiceCollection services = new();
        services.ConfigureXRestClientOptions(options =>
        {
            options.Timeout = TimeSpan.FromMinutes(2);
            options.EnableLogging = true;
            options.LogLevel = RestLogLevel.Debug;
            options.Retry = new RestRetryOptions
            {
                MaxRetryAttempts = 5,
                UseExponentialBackoff = false
            };
            options.CircuitBreaker = new RestCircuitBreakerOptions
            {
                FailureThreshold = 10,
                BreakDuration = TimeSpan.FromMinutes(1)
            };
        });

		ServiceProvider provider = services.BuildServiceProvider();

		// Act
		RestClientOptions options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<RestClientOptions>>().Value;

        // Assert
        options.Timeout.Should().Be(TimeSpan.FromMinutes(2));
        options.EnableLogging.Should().BeTrue();
        options.LogLevel.Should().Be(RestLogLevel.Debug);
        options.Retry.Should().NotBeNull();
        options.Retry!.MaxRetryAttempts.Should().Be(5);
        options.Retry.UseExponentialBackoff.Should().BeFalse();
        options.CircuitBreaker.Should().NotBeNull();
        options.CircuitBreaker!.FailureThreshold.Should().Be(10);
        options.CircuitBreaker.BreakDuration.Should().Be(TimeSpan.FromMinutes(1));
    }

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

    #endregion

    #region Test Helpers

    private sealed class DelegateDisposable(Action dispose) : IDisposable
    {
        public void Dispose() => dispose();
    }

    private sealed record SimpleRequest : IRestAttributeBuilder, IRestQueryString
    {
        public RestAttribute Build(IServiceProvider serviceProvider) =>
            new RestGetAttribute("/test");
        IDictionary<string, string?>? IRestQueryString.GetQueryString() => new Dictionary<string, string?>();
    }

    private sealed class SlowHandler : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Simulate a slow response
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json"),
                Version = HttpVersion.Version20
            };
        }
    }

    private sealed class FailingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Simulated network failure");
        }
    }

    #endregion
}
