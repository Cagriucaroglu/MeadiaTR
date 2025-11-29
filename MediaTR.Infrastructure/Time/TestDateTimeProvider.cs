using MediaTR.SharedKernel.Time;

namespace MediaTR.Infrastructure.Time;

/// <summary>
/// Test implementation of IDateTimeProvider with fixed time support
/// Useful for unit tests where deterministic time is required
/// </summary>
public sealed class TestDateTimeProvider : IDateTimeProvider
{
    private DateTimeOffset _fixedTime = DateTimeOffset.UtcNow;

    /// <summary>
    /// Sets a fixed time for all subsequent time queries
    /// </summary>
    /// <param name="time">The fixed time to use</param>
    public void SetFixedTime(DateTimeOffset time)
    {
        _fixedTime = time;
    }

    /// <summary>
    /// Advances the fixed time by the specified duration
    /// </summary>
    /// <param name="duration">Duration to advance</param>
    public void AdvanceTime(TimeSpan duration)
    {
        _fixedTime = _fixedTime.Add(duration);
    }

    /// <summary>
    /// Resets to current UTC time
    /// </summary>
    public void Reset()
    {
        _fixedTime = DateTimeOffset.UtcNow;
    }

    public DateTime Now => _fixedTime.LocalDateTime;

    public DateTime UtcNow => _fixedTime.UtcDateTime;

    public DateTimeOffset OffsetNow => _fixedTime.ToLocalTime();

    public DateTimeOffset OffsetUtcNow => _fixedTime;
}
