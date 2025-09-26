using MediaTR.SharedKernel;

namespace MediaTR.Domain.ValueObjects;

public class ProductAttribute : BaseValueObject
{
    public string Key { get; }
    public string Value { get; }
    public string Type { get; }
    public string? Unit { get; }

    public ProductAttribute(string key, string value, string type, string? unit = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Attribute key cannot be null or empty", nameof(key));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value cannot be null or empty", nameof(value));

        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Attribute type cannot be null or empty", nameof(type));

        Key = key;
        Value = value;
        Type = type;
        Unit = unit;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Key;
        yield return Value;
        yield return Type;
        yield return Unit ?? string.Empty;
    }
}