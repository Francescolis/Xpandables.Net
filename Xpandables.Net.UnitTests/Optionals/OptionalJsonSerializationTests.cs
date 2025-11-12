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
using System.Text.Json;
using System.Text.Json.Serialization;

using FluentAssertions;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.UnitTests.Optionals;

/// <summary>
/// Unit tests for Optional&lt;T&gt; JSON serialization and deserialization
/// using source-generated contexts for AOT compatibility.
/// </summary>
public class OptionalJsonSerializationTests
{
    private readonly JsonSerializerOptions _options;

    public OptionalJsonSerializationTests()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = { new OptionalJsonConverterFactory() }
        };

        // Add the source-generated contexts for AOT compatibility
        _options.TypeInfoResolverChain.Add(OptionalJsonContext.Default);
        _options.TypeInfoResolverChain.Add(TestJsonContext.Default);
    }

    #region Primitive Types Tests

    [Fact]
    public void Serialize_OptionalString_WithValue_ShouldSerializeValue()
    {
        // Arrange
        var optional = Optional.Some("Hello World");

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("\"Hello World\"");
    }

    [Fact]
    public void Serialize_OptionalString_Empty_ShouldSerializeNull()
    {
        // Arrange
        var optional = Optional.Empty<string>();

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("null");
    }

    [Fact]
    public void Deserialize_String_ShouldCreateOptionalWithValue()
    {
        // Arrange
        const string json = "\"Test String\"";

        // Act
        var optional = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Should().Be("Test String");
    }

    [Fact]
    public void Deserialize_Null_ShouldCreateEmptyOptional()
    {
        // Arrange
        const string json = "null";

        // Act
        var optional = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        optional.IsEmpty.Should().BeTrue();
    }

    [Fact]
    public void Serialize_OptionalInt_WithValue_ShouldSerializeValue()
    {
        // Arrange
        var optional = Optional.Some(42);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("42");
    }

    [Fact]
    public void Serialize_OptionalInt_Empty_ShouldSerializeNull()
    {
        // Arrange
        var optional = Optional.Empty<int>();

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("null");
    }

    [Fact]
    public void Deserialize_Int_ShouldCreateOptionalWithValue()
    {
        // Arrange
        const string json = "123";

        // Act
        var optional = JsonSerializer.Deserialize<Optional<int>>(json, _options);

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Should().Be(123);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RoundTrip_OptionalBool_ShouldPreserveValue(bool value)
    {
        // Arrange
        var optional = Optional.Some(value);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);
        var deserialized = JsonSerializer.Deserialize<Optional<bool>>(json, _options);

        // Assert
        deserialized.IsNotEmpty.Should().BeTrue();
        deserialized.Value.Should().Be(value);
    }

    [Fact]
    public void RoundTrip_OptionalDecimal_ShouldPreserveValue()
    {
        // Arrange
        var optional = Optional.Some(123.45m);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);
        var deserialized = JsonSerializer.Deserialize<Optional<decimal>>(json, _options);

        // Assert
        deserialized.IsNotEmpty.Should().BeTrue();
        deserialized.Value.Should().Be(123.45m);
    }

    [Fact]
    public void RoundTrip_OptionalDateTime_ShouldPreserveValue()
    {
        // Arrange
        var dateTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var optional = Optional.Some(dateTime);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);
        var deserialized = JsonSerializer.Deserialize<Optional<DateTime>>(json, _options);

        // Assert
        deserialized.IsNotEmpty.Should().BeTrue();
        deserialized.Value.Should().Be(dateTime);
    }

    [Fact]
    public void RoundTrip_OptionalGuid_ShouldPreserveValue()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var optional = Optional.Some(guid);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);
        var deserialized = JsonSerializer.Deserialize<Optional<Guid>>(json, _options);

        // Assert
        deserialized.IsNotEmpty.Should().BeTrue();
        deserialized.Value.Should().Be(guid);
    }

    #endregion

    #region Complex Types Tests

    [Fact]
    public void Serialize_OptionalCustomType_WithValue_ShouldSerializeObject()
    {
        // Arrange
        var person = new Person { Name = "John Doe", Age = 30, Email = "john@example.com" };
        var optional = Optional.Some(person);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);
        using var doc = JsonDocument.Parse(json);

        // Assert
        var root = doc.RootElement;
        root.GetProperty("name").GetString().Should().Be("John Doe");
        root.GetProperty("age").GetInt32().Should().Be(30);
        root.GetProperty("email").GetString().Should().Be("john@example.com");
    }

    [Fact]
    public void Serialize_OptionalCustomType_Empty_ShouldSerializeNull()
    {
        // Arrange
        var optional = Optional.Empty<Person>();

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("null");
    }

    [Fact]
    public void Deserialize_CustomType_ShouldCreateOptionalWithValue()
    {
        // Arrange
        const string json = """
            {
              "name": "Jane Smith",
              "age": 25,
              "email": "jane@example.com"
            }
            """;

        // Act
        var optional = JsonSerializer.Deserialize<Optional<Person>>(json, _options);

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Name.Should().Be("Jane Smith");
        optional.Value.Age.Should().Be(25);
        optional.Value.Email.Should().Be("jane@example.com");
    }

    [Fact]
    public void RoundTrip_OptionalCustomType_ShouldPreserveAllProperties()
    {
        // Arrange
        var address = new Address
        {
            Street = "123 Main St",
            City = "Springfield",
            ZipCode = "12345"
        };
        var optional = Optional.Some(address);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);
        var deserialized = JsonSerializer.Deserialize<Optional<Address>>(json, _options);

        // Assert
        deserialized.IsNotEmpty.Should().BeTrue();
        deserialized.Value.Street.Should().Be(address.Street);
        deserialized.Value.City.Should().Be(address.City);
        deserialized.Value.ZipCode.Should().Be(address.ZipCode);
    }

    #endregion

    #region Nested Optional Tests

    [Fact]
    public void Serialize_ObjectWithOptionalProperties_ShouldSerializeCorrectly()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = Optional.Some("test@example.com"),
            PhoneNumber = Optional.Empty<string>(),
            Age = Optional.Some(30)
        };

        // Act
        string json = JsonSerializer.Serialize(user, _options);
        using var doc = JsonDocument.Parse(json);

        // Assert
        var root = doc.RootElement;
        root.GetProperty("id").GetInt32().Should().Be(1);
        root.GetProperty("username").GetString().Should().Be("testuser");
        root.GetProperty("email").GetString().Should().Be("test@example.com");
        root.GetProperty("phoneNumber").ValueKind.Should().Be(JsonValueKind.Null);
        root.GetProperty("age").GetInt32().Should().Be(30);
    }

    [Fact]
    public void Deserialize_ObjectWithOptionalProperties_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = """
            {
              "id": 2,
              "username": "jane",
              "email": "jane@test.com",
              "phoneNumber": null,
              "age": 25
            }
            """;

        // Act
        var user = JsonSerializer.Deserialize<User>(json, _options);

        // Assert
        user.Should().NotBeNull();
        user!.Id.Should().Be(2);
        user.Username.Should().Be("jane");
        user.Email.IsNotEmpty.Should().BeTrue();
        user.Email.Value.Should().Be("jane@test.com");
        user.PhoneNumber.IsEmpty.Should().BeTrue();
        user.Age.IsNotEmpty.Should().BeTrue();
        user.Age.Value.Should().Be(25);
    }

    [Fact]
    public void RoundTrip_ObjectWithMixedOptionals_ShouldPreserveState()
    {
        // Arrange
        var product = new Product
        {
            Id = 100,
            Name = "Laptop",
            Description = Optional.Some("High-performance laptop"),
            Price = Optional.Some(1299.99m),
            Stock = Optional.Empty<int>(),
            Category = Optional.Some("Electronics")
        };

        // Act
        string json = JsonSerializer.Serialize(product, _options);
        var deserialized = JsonSerializer.Deserialize<Product>(json, _options);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(product.Id);
        deserialized.Name.Should().Be(product.Name);
        deserialized.Description.IsNotEmpty.Should().BeTrue();
        deserialized.Description.Value.Should().Be("High-performance laptop");
        deserialized.Price.IsNotEmpty.Should().BeTrue();
        deserialized.Price.Value.Should().Be(1299.99m);
        deserialized.Stock.IsEmpty.Should().BeTrue();
        deserialized.Category.IsNotEmpty.Should().BeTrue();
        deserialized.Category.Value.Should().Be("Electronics");
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Serialize_OptionalWithNullableValue_Empty_ShouldSerializeNull()
    {
        // Arrange
        var optional = Optional.Empty<string?>();

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("null");
    }

    [Fact]
    public void Serialize_ArrayOfOptionals_ShouldSerializeCorrectly()
    {
        // Arrange
        var optionals = new[]
        {
            Optional.Some(1),
            Optional.Empty<int>(),
            Optional.Some(3),
            Optional.Empty<int>(),
            Optional.Some(5)
        };

        // Act
        string json = JsonSerializer.Serialize(optionals, _options);
        using var doc = JsonDocument.Parse(json);

        // Assert
        var array = doc.RootElement;
        array.GetArrayLength().Should().Be(5);
        array[0].GetInt32().Should().Be(1);
        array[1].ValueKind.Should().Be(JsonValueKind.Null);
        array[2].GetInt32().Should().Be(3);
        array[3].ValueKind.Should().Be(JsonValueKind.Null);
        array[4].GetInt32().Should().Be(5);
    }

    [Fact]
    public void Deserialize_ArrayOfOptionals_ShouldDeserializeCorrectly()
    {
        // Arrange
        const string json = "[10, null, 20, null, 30]";

        // Act
        var optionals = JsonSerializer.Deserialize<Optional<int>[]>(json, _options);

        // Assert
        optionals.Should().NotBeNull();
        optionals!.Length.Should().Be(5);
        optionals[0].IsNotEmpty.Should().BeTrue();
        optionals[0].Value.Should().Be(10);
        optionals[1].IsEmpty.Should().BeTrue();
        optionals[2].IsNotEmpty.Should().BeTrue();
        optionals[2].Value.Should().Be(20);
        optionals[3].IsEmpty.Should().BeTrue();
        optionals[4].IsNotEmpty.Should().BeTrue();
        optionals[4].Value.Should().Be(30);
    }

    [Fact]
    public void Serialize_OptionalZeroValue_ShouldSerializeZero()
    {
        // Arrange
        var optional = Optional.Some(0);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("0");
    }

    [Fact]
    public void Serialize_OptionalEmptyString_ShouldSerializeEmptyString()
    {
        // Arrange
        var optional = Optional.Some(string.Empty);

        // Act
        string json = JsonSerializer.Serialize(optional, _options);

        // Assert
        json.Trim().Should().Be("\"\"");
    }

    [Fact]
    public void Deserialize_EmptyString_ShouldCreateOptionalWithEmptyString()
    {
        // Arrange
        const string json = "\"\"";

        // Act
        var optional = JsonSerializer.Deserialize<Optional<string>>(json, _options);

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Should().Be(string.Empty);
    }

    #endregion

    #region Stream Serialization Tests

    [Fact]
    public async Task SerializeAsync_OptionalToStream_ShouldWriteCorrectJson()
    {
        // Arrange
        var person = new Person { Name = "Alice", Age = 28, Email = "alice@test.com" };
        var optional = Optional.Some(person);
        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsync(stream, optional, _options);
        stream.Position = 0;
        using var doc = await JsonDocument.ParseAsync(stream);

        // Assert
        var root = doc.RootElement;
        root.GetProperty("name").GetString().Should().Be("Alice");
        root.GetProperty("age").GetInt32().Should().Be(28);
    }

    [Fact]
    public async Task DeserializeAsync_OptionalFromStream_ShouldReadCorrectly()
    {
        // Arrange
        const string json = """
            {
              "name": "Bob",
              "age": 35,
              "email": "bob@test.com"
            }
            """;
        using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json));

        // Act
        var optional = await JsonSerializer.DeserializeAsync<Optional<Person>>(stream, _options);

        // Assert
        optional.IsNotEmpty.Should().BeTrue();
        optional.Value.Name.Should().Be("Bob");
        optional.Value.Age.Should().Be(35);
    }

    [Fact]
    public async Task RoundTripAsync_OptionalViaStream_ShouldPreserveData()
    {
        // Arrange
        var address = new Address
        {
            Street = "456 Oak Ave",
            City = "Metropolis",
            ZipCode = "54321"
        };
        var optional = Optional.Some(address);
        using var stream = new MemoryStream();

        // Act
        await JsonSerializer.SerializeAsync(stream, optional, _options);
        stream.Position = 0;
        var deserialized = await JsonSerializer.DeserializeAsync<Optional<Address>>(stream, _options);

        // Assert
        deserialized.IsNotEmpty.Should().BeTrue();
        deserialized.Value.Street.Should().Be(address.Street);
        deserialized.Value.City.Should().Be(address.City);
        deserialized.Value.ZipCode.Should().Be(address.ZipCode);
    }

    #endregion

    #region Test Models

    public class Person
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Email { get; set; } = string.Empty;
    }

    public class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public Optional<string> Email { get; set; }
        public Optional<string> PhoneNumber { get; set; }
        public Optional<int> Age { get; set; }
    }

    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Optional<string> Description { get; set; }
        public Optional<decimal> Price { get; set; }
        public Optional<int> Stock { get; set; }
        public Optional<string> Category { get; set; }
        public Optional<Address> Address { get; set; }
    }

    #endregion
}

/// <summary>
/// Source-generated JSON context for test models to ensure AOT compatibility.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.Never)]
[JsonSerializable(typeof(OptionalJsonSerializationTests.Person))]
[JsonSerializable(typeof(OptionalJsonSerializationTests.Address))]
[JsonSerializable(typeof(OptionalJsonSerializationTests.User))]
[JsonSerializable(typeof(OptionalJsonSerializationTests.Product))]
[JsonSerializable(typeof(Optional<OptionalJsonSerializationTests.Person>))]
[JsonSerializable(typeof(Optional<OptionalJsonSerializationTests.Address>))]
[JsonSerializable(typeof(Optional<int>[]))]
public partial class TestJsonContext : JsonSerializerContext { }
