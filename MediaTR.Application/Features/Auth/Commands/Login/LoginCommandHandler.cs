using MediaTR.Domain.Repositories;
using MediaTR.Domain.Services;
using MediaTR.SharedKernel.Time;
using MediatR;

namespace MediaTR.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICacheService _cacheService;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IDateTimeProvider dateTimeProvider,
        ICacheService cacheService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _dateTimeProvider = dateTimeProvider;
        _cacheService = cacheService;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Try to find user by email first, then by username
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUsername, cancellationToken);

        if (user == null)
        {
            user = await _userRepository.GetByUsernameAsync(request.EmailOrUsername, cancellationToken);
        }

        if (user == null)
        {
            throw new InvalidOperationException("Auth.InvalidCredentials");
        }

        // Check if account is locked
        if (user.IsLockedOut())
        {
            throw new InvalidOperationException("Auth.AccountLocked");
        }

        // Check if account is active
        if (!user.IsActive)
        {
            throw new InvalidOperationException("Auth.AccountInactive");
        }

        // Verify password
        var isPasswordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!isPasswordValid)
        {
            // Increment failed login attempts
            user.IncrementFailedLogin();

            // Lock account after 5 failed attempts for 15 minutes
            if (user.FailedLoginAttempts >= 5)
            {
                user.LockAccount(15);
                await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
                throw new InvalidOperationException("Auth.AccountLockedDueToFailedAttempts");
            }

            await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);
            throw new InvalidOperationException("Auth.InvalidCredentials");
        }

        // Successful login - reset failed attempts and update last login
        user.ResetFailedLogin();
        user.LastLoginAt = _dateTimeProvider.OffsetUtcNow;
        await _userRepository.UpdateAsync(user, cancellationToken: cancellationToken);

        // Generate JWT tokens
        // Note: IP address should be passed from API layer, using placeholder for now
        var tokens = await _jwtTokenService.GenerateTokensAsync(user, "127.0.0.1", request.RememberMe, cancellationToken);

        // Cache user data for 15 minutes (performance optimization)
        await _cacheService.SetAsync(
            $"user:{user.Id}",
            user,
            TimeSpan.FromMinutes(15),
            cancellationToken);

        // Cache email → userId mapping for fast lookup (15 minutes)
        await _cacheService.SetAsync(
            $"user:email:{user.Email.Value.ToLowerInvariant()}",
            user.Id.ToString(),
            TimeSpan.FromMinutes(15),
            cancellationToken);

        // Cache username → userId mapping for fast lookup (15 minutes)
        await _cacheService.SetAsync(
            $"user:username:{user.UserName.ToLowerInvariant()}",
            user.Id.ToString(),
            TimeSpan.FromMinutes(15),
            cancellationToken);

        return new LoginResponse(
            user.Id,
            user.UserName,
            user.Email.Value,
            tokens.AccessToken,
            tokens.RefreshToken,
            tokens.AccessTokenExpiresAt,
            tokens.RefreshTokenExpiresAt);
    }
}
