using FluentValidation;

namespace Product.Application.CreateProduct;

public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    private const int MaxNameLength = 100;
    private const int MaxDescriptionLength = 2_000;
    
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(MaxNameLength);
        
        RuleFor(x => x.Description)
            .MaximumLength(MaxDescriptionLength);
        
        RuleFor(x => x.Price)
            .GreaterThan(0);
    }
}