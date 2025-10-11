#pragma warning disable S109

namespace MediaTR.Infrastructure.Data.EntityConfigurations;

using MediaTR.Domain.Events.Entities;
using MediaTR.SharedKernel.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Entity Framework Core configuration for OutboxEvent entity.
/// Configures table structure, indexes, and constraints for the Outbox Pattern implementation.
/// </summary>
public sealed class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Table name
        builder.ToTable("OutboxEvents");

        // Primary key
        builder.HasKey(e => e.Id);

        // Properties
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever(); // We generate Guid.CreateVersion7() in domain

        builder.Property(e => e.EventType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.AggregateId)
            .IsRequired(false); // Nullable for bulk/cross-aggregate operations

        builder.Property(e => e.AggregateType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.Payload)
            .IsRequired()
            .HasMaxLength(int.MaxValue); // NVARCHAR(MAX) for JSON payload

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>() // Store enum as string for readability
            .HasMaxLength(20);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.Property(e => e.ProcessedAt);

        builder.Property(e => e.RetryCount)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(e => e.MaxRetries)
            .IsRequired()
            .HasDefaultValue(5);

        builder.Property(e => e.LastError)
            .HasMaxLength(2000);

        builder.Property(e => e.CorrelationId);

        builder.Property(e => e.Metadata)
            .HasMaxLength(4000);

        builder.Property(e => e.ConsistencyLevel)
            .IsRequired()
            .HasConversion<string>() // Store enum as string for readability
            .HasMaxLength(20)
            .HasDefaultValue(ConsistencyLevel.Eventual);

        // Indexes for efficient querying
        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_OutboxEvents_Status");

        builder.HasIndex(e => e.EventType)
            .HasDatabaseName("IX_OutboxEvents_EventType");

        builder.HasIndex(e => new { e.Status, e.CreatedAt })
            .HasDatabaseName("IX_OutboxEvents_Status_CreatedAt");

        // Composite index for background processing queries
        builder.HasIndex(e => new { e.Status, e.RetryCount })
            .HasDatabaseName("IX_OutboxEvents_Status_RetryCount")
            .HasFilter("[Status] = 'Pending'"); // Filtered index for pending items

        // Index for ordered processing with retry consideration
        builder.HasIndex(e => new { e.Status, e.RetryCount, e.CreatedAt })
            .HasDatabaseName("IX_OutboxEvents_Processing");

        builder.HasIndex(e => e.CorrelationId)
            .HasDatabaseName("IX_OutboxEvents_CorrelationId");

        builder.HasIndex(e => new { e.AggregateType, e.AggregateId })
            .HasDatabaseName("IX_OutboxEvents_Aggregate");
    }
}
