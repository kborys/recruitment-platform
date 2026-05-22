namespace Product.Api.Products;

public record CreateProductRequest(
    string Name,
    string? Description,
    decimal Price
);
