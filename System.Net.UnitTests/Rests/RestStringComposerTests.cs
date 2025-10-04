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
using System.Net.Rests.RequestBuilders;
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Xpandables.Net.Rests;

namespace System.Net.UnitTests.Rests;

public partial class RestStringComposerTests
{
    [Fact]
    public async Task Compose_WithBodyAndStringFormat_ShouldSetStringContent()
    {
        // Arrange
        var attribute = new RestPostAttribute("/")
        {
            Location = RestSettings.Location.Body,
            BodyFormat = RestSettings.BodyFormat.String,
            ContentType = RestSettings.ContentType.Json
        };

        var message = new HttpRequestMessage(HttpMethod.Post, "/");
        var request = new TestStringRequest("hello");
        var context = new RestRequestContext
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                TypeInfoResolver = TestStringRequestJsonContext.Default
            }
        };

        var composer = new RestStringComposer<TestStringRequest>();

        // Act
        composer.Compose(context);

        // Assert
        context.Message.Content.Should().BeOfType<StringContent>();
        var content = (StringContent)context.Message.Content!;
        content.Headers.ContentType!.MediaType.Should().Be(RestSettings.ContentType.Json);
        var body = await content.ReadAsStringAsync();
        body.Should().Contain("\"hello\"");
    }

    [Fact]
    public async Task Compose_WithMultipart_ShouldAddAsPart()
    {
        // Arrange
        var attribute = new RestPostAttribute("/")
        {
            Location = RestSettings.Location.Body,
            BodyFormat = RestSettings.BodyFormat.String,
            ContentType = RestSettings.ContentType.Json
        };

        var message = new HttpRequestMessage(HttpMethod.Post, "/")
        {
            Content = new MultipartFormDataContent()
        };
        var request = new TestStringRequest("world");
        var context = new RestRequestContext
        {
            Attribute = attribute,
            Message = message,
            Request = request,
            SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                TypeInfoResolver = TestStringRequestJsonContext.Default
            }
        };

        var composer = new RestStringComposer<TestStringRequest>();

        // Act
        composer.Compose(context);

        // Assert
        context.Message.Content.Should().BeOfType<MultipartFormDataContent>();
        var multipart = (MultipartFormDataContent)context.Message.Content!;
        var parts = multipart.ToList();
        parts.Should().HaveCount(1);
        var str = await parts[0].ReadAsStringAsync();
        str.Should().Contain("\"world\"");
    }

    private sealed record TestStringRequest(string Value) : IRestString
    {
        public object GetStringContent() => this;
    }

    [JsonSerializable(typeof(TestStringRequest))]
    private sealed partial class TestStringRequestJsonContext : JsonSerializerContext
    {
    }
}
