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
using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Optionals;
using Xpandables.Net.Primitives.Text;

namespace Xpandables.Net.UnitTests;

readonly record struct StructType(string Value);
abstract record class Vehicle(string Name);
sealed record class Car() : Vehicle(nameof(Car));
sealed record class Truck() : Vehicle(nameof(Car));

sealed record class Cache<T>
     where T : class
{
    private readonly Dictionary<string, T> data = [];
    public void Store(string key, T value) => data[key] = value;
    public Optional<T> Get(string key) => data.TryGetValue(key, out var value) ? value : default;
}

public sealed class OptionalUnitTest
{
    const string Value = "Hello World";
    [Theory]
    [InlineData(Value)]
    public void OptionalJsonConverter_Should_Serialize_And_Deserialize_Optional(string value)
    {
        var optional = Optional.Some(new StructType(value));
        var json = JsonSerializer.Serialize(optional);
        var deserialized = JsonSerializer.Deserialize<Optional<StructType>>(json);

        var op = Optional.Some(new { Name = value });
        var json1 = JsonSerializer.Serialize(op);
        var deserialized1 = json1.DeserializeAnonymousType(op);

        optional.Should().BeEquivalentTo(deserialized);
        op.Should().BeEquivalentTo(deserialized1);
    }

    [Fact]
    public void Optional_Should_Return_Empty_Optional_When_StructValue_Is_Null()
    {
        var optional = Optional.Empty<StructType>();

        optional.Should().BeEmpty();
    }

    [Theory]
    [InlineData(Value)]
    public void Optional_Should_Return_Some_Optional_When_StructValue_Is_NotNull(string value)
    {
        var optional = Optional.Empty<StructType>();

        optional.Map(_ => new StructType(value)).Should().BeEmpty();
        optional.Reduce(() => new StructType(value)).Should().NotBeEmpty();
    }

    [Theory]
    [InlineData("car", "truck", "bus", "My favorite vehicle is {0}", "No favorite vehicle")]
    public void Optional_Should_Chain_Return_Some_Or_Empty_According_To_Value(
        string car, string truck, string bus, string favorite, string noFavorite)
    {
        Cache<string> cache = new();
        cache.Store("a", car);
        cache.Store("b", truck);
        cache.Store("c", bus);

        var noFavoriteMessage = cache
            .Get("d")
            .Map(vehicle => string.Format(favorite, vehicle))
            .Reduce(() => noFavorite);

        var favoriteTruckMessage = cache
            .Get("b")
            .Map(vehicle => string.Format(favorite, vehicle))
            .Reduce(() => noFavorite);

        noFavoriteMessage.Should().BeEquivalentTo(noFavorite);
        favoriteTruckMessage.Should().BeEquivalentTo(string.Format(favorite, truck));
    }
}
