using FluentValidation;
using MediatR;
using MediaTR.SharedKernel.ResultAndError;

namespace MediaTR.Application.Behaviors;

/// <summary>
/// MediatR pipeline behavior for request validation using FluentValidation
/// Runs before command/query handlers to ensure input validity
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResult
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Skip validation if no validators registered
        if (!_validators.Any())
        {
            return await next();
        }

        // Create validation context
        var context = new ValidationContext<TRequest>(request);

        // Run all validators in parallel
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all failures
        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        // If validation failed, return ValidationError
        if (failures.Count != 0)
        {
            var errors = failures
                .Select(f => $"{f.PropertyName}: {f.ErrorMessage}")
                .ToArray();

            return CreateValidationResult<TResponse>(errors);
        }

        return await next();
    }

    private static TResult CreateValidationResult<TResult>(string[] errors)
        where TResult : IResult
    {
        // For Result<T>
        if (typeof(TResult).IsGenericType)
        {
            var resultType = typeof(Result<>).MakeGenericType(
                typeof(TResult).GenericTypeArguments[0]);

            var failureMethod = resultType
                .GetMethod(nameof(Result.Failure))!
                .MakeGenericMethod(typeof(TResult).GenericTypeArguments[0]);

            var error = Error.Validation("Validation.Failed", string.Join("; ", errors));

            return (TResult)failureMethod.Invoke(null, new object[] { error })!;
        }

        // For Result
        return (TResult)(object)Result.Failure(
            Error.Validation("Validation.Failed", string.Join("; ", errors)));
    }
}
