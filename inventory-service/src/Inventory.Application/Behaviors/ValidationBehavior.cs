using FluentValidation;
using MediatR;

namespace Inventory.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(
    IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }
        
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(request, cancellationToken)));

        var validationErrors = validationResults.Where(x => !x.IsValid)
            .SelectMany(x => x.Errors)
            .ToArray();

        if (validationErrors.Length != 0)
        {
            throw new ValidationException(validationErrors);
        }
        
        return await next(cancellationToken);
    }
}