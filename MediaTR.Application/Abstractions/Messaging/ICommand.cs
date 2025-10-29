using MediatR;
using MediaTR.SharedKernel.BusinessLogic;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Command wrapper interface - BusinessLogicContext taşır
/// </summary>
public interface ICommandWrapper
{
    BusinessLogicContext BusinessLogicContext { get; }
}

/// <summary>
/// Generic command wrapper base class
/// </summary>
public abstract record CommandWrapper<TRequest, TResponse> : ICommand<TResponse>, ICommandWrapper
{
    protected CommandWrapper(TRequest request, Guid correlationId)
    {
        Request = request;
        BusinessLogicContext = new BusinessLogicContext
        {
            CorrelationId = correlationId
        };
    }

    public TRequest Request { get; init; }
    public BusinessLogicContext BusinessLogicContext { get; init; }
}