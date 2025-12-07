using MediaTR.Domain.Entities;
using MediaTR.Domain.Enums;
using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;
using MediaTR.Domain.ValueObjects;
using MediaTR.SharedKernel.Time;
using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // Check if email already exists
        var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUserByEmail != null)
        {
            throw new InvalidOperationException("Auth.Email.AlreadyExists");
        }

        // Check if username already exists
        var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.UserName, cancellationToken);
        if (existingUserByUsername != null)
        {
            throw new InvalidOperationException("Auth.UserName.AlreadyExists");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = Email.Create(request.Email),
            PasswordHash = passwordHash,
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.UserName,
            PhoneNumber = request.PhoneNumber,
            Role = UserRole.Customer, // Default role
            IsActive = true,
            IsEmailVerified = false, // Will be verified via email confirmation
            CreatedAt = _dateTimeProvider.OffsetUtcNow,
            LastPasswordChangedAt = _dateTimeProvider.OffsetUtcNow
        };

        // Save to MongoDB
        await _userRepository.AddAsync(user, cancellationToken: cancellationToken);

        // Generate JWT tokens (will also save refresh token to SQL Server)
        // Note: IP address should be passed from API layer, using placeholder for now
        var tokens = await _jwtTokenService.GenerateTokensAsync(user, "127.0.0.1", cancellationToken);

        return new RegisterResponse(
            user.Id,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshTokenExpiresAt);
    }
}
