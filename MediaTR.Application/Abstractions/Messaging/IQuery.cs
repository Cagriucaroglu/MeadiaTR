using MediatR;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}