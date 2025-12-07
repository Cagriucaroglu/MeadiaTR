using MediaTR.Domain.Enums;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Email Email { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public Address? BillingAddress { get; set; }
    public Address? ShippingAddress { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public bool IsEmailVerified { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset? LastLoginAt { get; set; }

    // Authentication security fields
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTimeOffset? LockoutEnd { get; set; }
    public DateTimeOffset? LastPasswordChangedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";

    // Helper methods for authentication
    public bool IsLockedOut() => LockoutEnd.HasValue && LockoutEnd.Value > DateTimeOffset.UtcNow;
    public void IncrementFailedLogin() => FailedLoginAttempts++;
    public void ResetFailedLogin() => FailedLoginAttempts = 0;
    public void LockAccount(int minutes) => LockoutEnd = DateTimeOffset.UtcNow.AddMinutes(minutes);
}