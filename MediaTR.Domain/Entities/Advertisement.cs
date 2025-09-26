using MediaTR.Domain.Enums;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class Advertisement : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid ProductId { get; set; } = Guid.NewGuid();
    public Product? Product { get; set; }
    public Guid CategoryId { get; set; } = Guid.NewGuid();
    public Category? Category { get; set; }
    public Guid UserId { get; set; } = Guid.NewGuid();
    public Guid SellerId { get; set; } = Guid.NewGuid();
    public User? Seller { get; set; }
    public Money Price { get; set; } 
    public AdvertisementStatus Status { get; set; } = AdvertisementStatus.Draft;
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public int ViewCount { get; set; }
    public string? MainImageUrl { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public bool IsNegotiable { get; set; }
    public bool IsUrgent { get; set; }
    public bool IsFeatured { get; set; }

    private readonly List<string> _imageUrls = [];
    public IReadOnlyCollection<string> ImageUrls => _imageUrls.AsReadOnly();

    private readonly List<string> _tags = [];
    public IReadOnlyCollection<string> Tags => _tags.AsReadOnly();

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
    public bool IsActiveStatus => Status == AdvertisementStatus.Active && !IsExpired;
}