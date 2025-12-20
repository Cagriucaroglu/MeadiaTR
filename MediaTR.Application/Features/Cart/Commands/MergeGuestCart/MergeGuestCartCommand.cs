using MediatR;

namespace MediaTR.Application.Features.Cart.Commands.MergeGuestCart;

/// <summary>
/// Merge guest cart into user cart after login
/// </summary>
public record MergeGuestCartCommand(string GuestSessionId) : IRequest<bool>;
