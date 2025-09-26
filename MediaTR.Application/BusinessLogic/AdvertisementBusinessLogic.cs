using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Events;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Application.BusinessLogic;

public class AdvertisementBusinessLogic
{
    public Advertisement CreateAdvertisement(string title, string description, Guid productId, Guid sellerId,
        Money price, bool isNegotiable = false, bool isUrgent = false, string? contactPhone = null, string? contactEmail = null)
    {
        // Business validation
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Advertisement title cannot be empty", nameof(title));

        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID cannot be empty", nameof(productId));

        if (sellerId == Guid.Empty)
            throw new ArgumentException("Seller ID cannot be empty", nameof(sellerId));

        // Create advertisement entity
        var advertisement = new Advertisement
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            ProductId = productId,
            SellerId = sellerId,
            Price = price,
            IsNegotiable = isNegotiable,
            IsUrgent = isUrgent,
            ContactPhone = contactPhone,
            ContactEmail = contactEmail,
            Status = AdvertisementStatus.Draft,
            ViewCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        return advertisement;
    }

    public void SubmitForApproval(Advertisement advertisement)
    {
        // Business rule validation
        if (advertisement.Status != AdvertisementStatus.Draft)
            throw new InvalidOperationException("Only draft advertisements can be submitted for approval");

        advertisement.Status = AdvertisementStatus.PendingApproval;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Advertisement advertisement)
    {
        // Business rule validation
        if (advertisement.Status != AdvertisementStatus.PendingApproval)
            throw new InvalidOperationException("Only pending advertisements can be approved");

        advertisement.Status = AdvertisementStatus.Active;
        advertisement.PublishedAt = DateTime.UtcNow;
        advertisement.ExpiresAt = DateTime.UtcNow.AddDays(30); // Default 30 days
        advertisement.UpdatedAt = DateTime.UtcNow;

        // Raise domain event
        advertisement.Raise(new AdvertisementPublishedEvent(
            advertisement.Id, advertisement.ProductId, advertisement.SellerId, advertisement.Price, advertisement.Title));
    }

    public void Reject(Advertisement advertisement)
    {
        // Business rule validation
        if (advertisement.Status != AdvertisementStatus.PendingApproval)
            throw new InvalidOperationException("Only pending advertisements can be rejected");

        advertisement.Status = AdvertisementStatus.Rejected;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(Advertisement advertisement)
    {
        // Business rule validation
        if (advertisement.Status != AdvertisementStatus.Active)
            throw new InvalidOperationException("Only active advertisements can be deactivated");

        advertisement.Status = AdvertisementStatus.Inactive;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold(Advertisement advertisement)
    {
        // Business rule validation
        if (advertisement.Status != AdvertisementStatus.Active)
            throw new InvalidOperationException("Only active advertisements can be marked as sold");

        advertisement.Status = AdvertisementStatus.Sold;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViewCount(Advertisement advertisement)
    {
        advertisement.ViewCount++;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePrice(Advertisement advertisement, Money newPrice)
    {
        // Business rule: Only active or draft advertisements can have price updated
        if (advertisement.Status != AdvertisementStatus.Active &&
            advertisement.Status != AdvertisementStatus.Draft)
            throw new InvalidOperationException("Only active or draft advertisements can have price updated");

        advertisement.Price = newPrice;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void SetMainImage(Advertisement advertisement, string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));

        advertisement.MainImageUrl = imageUrl;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateContactInfo(Advertisement advertisement, string? contactPhone, string? contactEmail)
    {
        advertisement.ContactPhone = contactPhone;
        advertisement.ContactEmail = contactEmail;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void SetUrgent(Advertisement advertisement, bool isUrgent)
    {
        advertisement.IsUrgent = isUrgent;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }

    public void SetNegotiable(Advertisement advertisement, bool isNegotiable)
    {
        advertisement.IsNegotiable = isNegotiable;
        advertisement.UpdatedAt = DateTime.UtcNow;
    }
}