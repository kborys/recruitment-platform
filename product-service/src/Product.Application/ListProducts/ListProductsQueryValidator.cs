using FluentValidation;

namespace Product.Application.ListProducts;

public class ListProductsQueryValidator : AbstractValidator<ListProductsQuery>
{
    private const int MaxPageSize = 100;
    
    public ListProductsQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);
        
        RuleFor(x => x.PageSize)
            .GreaterThan(0)
            .LessThanOrEqualTo(MaxPageSize);
    }
}