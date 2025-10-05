/*******************************************************************************
* Copyright (C) 2025 Francis-Black EWANE
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
using FluentAssertions;

using Xpandables.Net.Rests;
using Xpandables.Net.Rests.RequestBuilders;

namespace Xpandables.Net.UnitTests.Rests;

public class RestStreamComposerTests
{
    [Fact]
    public void Compose_WithBodyAndStreamFormat_ShouldSetStreamContent()
    {
        // Arrange
        var attribute = new RestPostAttribute("/")
        {
            Location = RestSettings.Location.Body,
            BodyFormat = RestSettings.BodyFormat.Stream,
            ContentType = RestSettings.ContentType.OctetStream
        };

        var message = new HttpRequestMessage(HttpMethod.Post, "/");
        var request = new TestStreamRequest(new MemoryStream([1, 2, 3]));
        var context = new RestRequestContext
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = RestSettings.SerializerOptions
        };

        var composer = new RestStreamComposer<TestStreamRequest>();

        // Act
        composer.Compose(context);

        // Assert
        context.Message.Content.Should().BeOfType<StreamContent>();
    }

    [Fact]
    public void Compose_WithMultipart_ShouldAddStreamPart()
    {
        // Arrange
        var attribute = new RestPostAttribute("/")
        {
            Location = RestSettings.Location.Body,
            BodyFormat = RestSettings.BodyFormat.Stream
        };

        var message = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = new MultipartFormDataContent()
        };
        var request = new TestStreamRequest(new MemoryStream([10, 20]));
        var context = new RestRequestContext
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = RestSettings.SerializerOptions
        };

        var composer = new RestStreamComposer<TestStreamRequest>();

        // Act
        composer.Compose(context);

        // Assert
        context.Message.Content.Should().BeOfType<MultipartFormDataContent>();
        var multipart = (MultipartFormDataContent)context.Message.Content!;
        multipart.Count().Should().Be(1);
    }

    private sealed class TestStreamRequest(Stream stream) : IRestStream
    {
        public StreamContent GetStreamContent() => new(stream);
    }
}
