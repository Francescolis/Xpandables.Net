using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Xpandables.Net.SampleApi.EnumerationTypes;

[PrimitiveJsonConverter<AccountType, string>]
[TypeConverter(typeof(PrimitiveTypeConverter<AccountType, string>))]
public readonly record struct AccountType : IPrimitive<AccountType, string>
{
    public static readonly AccountType Savings = new("Savings");
    public static readonly AccountType Checking = new("Checking");
    public static readonly AccountType Business = new("Business");
    public string Value { get; }
    private AccountType(string value) => Value = value;
    public override string ToString() => Value;
    public static implicit operator string(AccountType self) => self.Value;
    public static string DefaultValue => Savings;
    public static implicit operator AccountType(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Create(value);
    }
    public static bool TryParse(string? s, IFormatProvider? provider, out AccountType result)
    {
        result = default;
        if (string.IsNullOrWhiteSpace(s))
        {
            return false;
        }
        try
        {
            result = Create(s);
            return true;
        }
        catch (ValidationException)
        {
            return false;
        }
    }
    public bool Equals(AccountType other) => Value.Equals(other.Value, StringComparison.Ordinal);
    public override int GetHashCode() => Value.GetHashCode(StringComparison.Ordinal);
    public static AccountType Create(string value)
    {
        return value.ToLowerInvariant() switch
        {
            "savings" => Savings,
            "checking" => Checking,
            "business" => Business,
            _ => throw new ValidationException(
                new ValidationResult($"'{value}' is not a valid account type.", [nameof(AccountType)]), null, value)
        };
    }
}
