namespace Product.Application.GetProductById;

public record ProductDto(Guid Id, string Name, string? Description, decimal Price, int Amount, DateTime CreatedAt, DateTime UpdatedAt);