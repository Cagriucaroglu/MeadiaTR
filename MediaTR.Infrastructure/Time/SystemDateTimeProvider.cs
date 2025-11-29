using MediaTR.SharedKernel.Time;

namespace MediaTR.Infrastructure.Time;

/// <summary>
/// Production implementation of IDateTimeProvider using system time
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTimeOffset OffsetNow => DateTimeOffset.Now;

    public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
}
