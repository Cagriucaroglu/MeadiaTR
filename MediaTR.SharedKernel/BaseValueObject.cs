namespace MediaTR.SharedKernel;

public abstract class BaseValueObject : IEquatable<BaseValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();

    public override bool Equals(object? obj)
    {
        if (obj == null || obj.GetType() != GetType())
        {
            return false;
        }

        var other = (BaseValueObject)obj;
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public bool Equals(BaseValueObject? other)
    {
        return Equals((object?)other);
    }

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }

    public static bool operator ==(BaseValueObject left, BaseValueObject right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseValueObject left, BaseValueObject right)
    {
        return !Equals(left, right);
    }
}