using System.Text.Json;

using FluentAssertions;

using Xpandables.Net.Optionals;

namespace Xpandables.Net.Test.UnitTests;
public sealed class OptionalUnitTest
{
    const string Value = "Hello World";

    [Theory]
    [InlineData(Value)]
    public void OptionalJsonConverter_Should_Serialize_And_Deserialize_Optional(
        string value)
    {
        Optional<StructType> optional = Optional.Some(new StructType(value));
        string json = JsonSerializer.Serialize(optional);
        Optional<StructType> deserialized = JsonSerializer
            .Deserialize<Optional<StructType>>(json);

        var op = Optional.Some(new { Name = value });
        string json1 = JsonSerializer.Serialize(op);
        var deserialized1 = DeserializeAnonymousType(json1, op);

        optional.Should().BeEquivalentTo(deserialized);
        op.Should().BeEquivalentTo(deserialized1);
    }

    [Fact]
    public void Optional_Should_Return_Empty_Optional_When_StructValue_Is_Null()
    {
        Optional<StructType> optional = Optional.Empty<StructType>();

        optional.IsEmpty.Should().BeTrue();
    }

    [Theory]
    [InlineData(4, 6, 10)]
    [InlineData(4, 0, 4)]
    [InlineData(0, 6, 6)]
    [InlineData(0, 0, 0)]
    public void Optional_Should_Return_Some_Optional_When_On_Link_Syntax(
        int fromA, int fromB, int result)
    {
        Optional<int> optional = from a in Optional.Some(fromA)
                                 from b in Optional.Some(fromB)
                                 select a + b;

        optional.Value.Should().Be(result);
    }

    [Theory]
    [InlineData(42, true)]
    [InlineData(43, false)]
    public void Optional_Should_Return_Boolean_Optional_When_Bind_To_Boolean(
        int x, bool result)
    {
        Optional<bool> optional = Optional
            .Some(x)
            .Bind(value => value % 2 == 0);

        optional.Value.Should().Be(result);
    }

    [Theory]
    [InlineData(Value)]
    public void Optional_Should_Return_Some_Optional_When_StructValue_Is_NotNull(
        string value)
    {
        Optional<StructType> optional = Optional.Empty<StructType>();

        optional.Map(_ => new StructType(value)).IsEmpty.Should().BeTrue();
        optional.Empty(() => new StructType(value)).IsNotEmpty.Should().BeTrue();
    }

    [Theory]
    [InlineData("car", "truck", "bus", "My favorite vehicle is {0}",
        "No favorite vehicle")]
    public void Optional_Should_Chain_Return_Some_Or_Empty_According_To_Value(
        string car, string truck, string bus, string favorite, string noFavorite)
    {
        Cache<string> cache = new();
        cache.Store("a", car);
        cache.Store("b", truck);
        cache.Store("c", bus);

        Optional<string> noFavoriteMessage = cache
            .Get("d")
            .Map(vehicle => string.Format(favorite, vehicle))
            .Empty(() => noFavorite);

        Optional<string> favoriteTruckMessage = cache
            .Get("b")
            .Map(vehicle => string.Format(favorite, vehicle))
            .Empty(() => noFavorite);

        noFavoriteMessage.Should().BeEquivalentTo(noFavorite);
        favoriteTruckMessage.Should().BeEquivalentTo(
            string.Format(favorite, truck));
    }

    private static T? DeserializeAnonymousType<T>(
        string json,
        T _,
        JsonSerializerOptions? options = default) =>
         JsonSerializer.Deserialize<T>(json, options);
}

readonly record struct StructType(string Name);
abstract record class Vehicle(string Name);
sealed record class Car() : Vehicle(nameof(Car));
sealed record class Truck() : Vehicle(nameof(Car));

sealed record class Cache<T>
     where T : class
{
    private readonly Dictionary<string, T> data = [];
    public void Store(string key, T value) => data[key] = value;
    public Optional<T> Get(string key)
        => data.TryGetValue(key, out T? value) ? value : default;
}
