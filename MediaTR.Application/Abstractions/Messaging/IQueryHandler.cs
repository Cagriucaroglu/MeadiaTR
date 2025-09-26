using MediatR;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Abstractions.Messaging;

public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}