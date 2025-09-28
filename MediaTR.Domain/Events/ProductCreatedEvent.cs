using MediaTR.Domain.Entities;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Events;

public record ProductCreatedEvent : DomainEvent<Product>;