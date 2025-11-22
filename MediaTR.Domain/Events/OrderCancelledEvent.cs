using MediaTR.Domain.Entities;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Events;

public record OrderCancelledEvent : DomainEvent<Order>;
