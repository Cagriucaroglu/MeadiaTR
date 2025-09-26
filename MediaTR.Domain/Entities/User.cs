using MediaTR.Domain.Enums;
using MediaTR.Domain.ValueObjects;
using MediaTR.Domain.Events;
using MediaTR.SharedKernel;

namespace MediaTR.Domain.Entities;

public class User : BaseEntity
{
    public string UserName { get; set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Address? BillingAddress { get; private set; }
    public Address? ShippingAddress { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    private User() { } // EF Constructor

    private User(string firstName, string lastName, Email email, string passwordHash, UserRole role)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        IsEmailVerified = false;
        IsActive = true;
        
        AddDomainEvent(new UserRegisteredEvent(Id, Email, FirstName, LastName));
    }

    public static User Create(string firstName, string lastName, Email email, string passwordHash, UserRole role = UserRole.Customer)
    {
        return new User(firstName, lastName, email, passwordHash, role);
    }

    // Simple state changes - business logic in Application layer
    public void SetEmailVerified(bool isVerified)
    {
        IsEmailVerified = isVerified;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";
}