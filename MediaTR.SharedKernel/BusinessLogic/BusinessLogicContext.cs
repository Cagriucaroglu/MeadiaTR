using MediaTR.SharedKernel.Data;
using MediaTR.SharedKernel.Outbox;

namespace MediaTR.SharedKernel.BusinessLogic;

/// <summary>
/// Business logic operations için context sağlar
/// Transaction yönetimi ve OutboxEvent tracking yapar
/// </summary>
public sealed class BusinessLogicContext
{
    private readonly List<IOutboxEvent> _trackedOutboxEvents = [];

    /// <summary>
    /// Transaction durumu
    /// </summary>
    public bool IsInTransaction { get; set; }

    /// <summary>
    /// Aktif transaction
    /// </summary>
    public IDbTransaction? Transaction { get; set; }

    /// <summary>
    /// Correlation ID (distributed tracing için)
    /// </summary>
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// OutboxEvent'i track listesine ekler
    /// </summary>
    public void TrackOutboxEvent(IOutboxEvent outboxEvent)
    {
        ArgumentNullException.ThrowIfNull(outboxEvent);
        _trackedOutboxEvents.Add(outboxEvent);
    }

    /// <summary>
    /// Track edilen tüm OutboxEvent'leri döner
    /// </summary>
    public IReadOnlyList<IOutboxEvent> GetTrackedOutboxEvents() => _trackedOutboxEvents.AsReadOnly();

    /// <summary>
    /// Track listesini temizler
    /// </summary>
    public void ClearTrackedOutboxEvents() => _trackedOutboxEvents.Clear();
}
