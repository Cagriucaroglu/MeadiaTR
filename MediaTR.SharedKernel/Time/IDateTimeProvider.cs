namespace MediaTR.SharedKernel.Time;

/// <summary>
/// Provides abstraction over DateTime/DateTimeOffset for testability
/// </summary>
public interface IDateTimeProvider
{
    /// <summary>
    /// Gets the current local date and time
    /// </summary>
    DateTime Now { get; }

    /// <summary>
    /// Gets the current UTC date and time
    /// </summary>
    DateTime UtcNow { get; }

    /// <summary>
    /// Gets the current local date and time with offset
    /// </summary>
    DateTimeOffset OffsetNow { get; }

    /// <summary>
    /// Gets the current UTC date and time with offset
    /// </summary>
    DateTimeOffset OffsetUtcNow { get; }
}
