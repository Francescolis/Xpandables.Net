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

    private static RestResponseContext CreateContext(HttpStatusCode statusCode = HttpStatusCode.OK) => new()
    {
        Request = new TestRequest(),
        Message = new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(string.Empty)
        },
        SerializerOptions = RestSettings.SerializerOptions
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
}
