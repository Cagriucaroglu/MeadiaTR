using MediaTR.Domain.Events.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MediaTR.Infrastructure.Configuration;

/// <summary>
/// EF Core entity configuration for OutboxEvent.
/// Defines database schema, indexes, and constraints for the outbox pattern.
/// </summary>
public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("OutboxEvents");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.AggregateType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.AggregateId)
            .IsRequired(false);

        builder.Property(e => e.Payload)
            .IsRequired()
            .HasMaxLength(int.MaxValue);

        builder.Property(e => e.Status)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>();

        builder.Property(e => e.ConsistencyLevel)
            .IsRequired()
            .HasMaxLength(20)
            .HasConversion<string>()
            .HasDefaultValue(SharedKernel.Outbox.ConsistencyLevel.Eventual);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ProcessedAt)
            .IsRequired(false);

        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.MaxRetries)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(e => e.LastError)
            .IsRequired(false)
            .HasMaxLength(2000);

        builder.Property(e => e.CorrelationId)
            .IsRequired(false);

        builder.Property(e => e.Metadata)
            .IsRequired(false)
            .HasMaxLength(4000);

        // Indexes for query performance
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_OutboxEvents_Status");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_OutboxEvents_EventType");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("IX_OutboxEvents_CorrelationId");

        builder.HasIndex(e => new { e.AggregateType, e.AggregateId })
            .HasDatabaseName("IX_OutboxEvents_Aggregate");

        // Critical index for background processor queries
        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_OutboxEvents_Status_CreatedAt");

        // Composite index for efficient pending event retrieval
        builder.HasIndex(e => new { e.Status, e.RetryCount, e.CreatedAt })
            .HasDatabaseName("IX_OutboxEvents_Processing");

        // Filtered index for pending events only (SQL Server specific)
        builder.HasIndex(e => new { e.Status, e.RetryCount })
            .HasDatabaseName("IX_OutboxEvents_Status_RetryCount")
            .HasFilter("[Status] = 'Pending'");
    }
}
