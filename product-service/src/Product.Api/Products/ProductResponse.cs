namespace Product.Api.Products;

public record ProductResponse(Guid Id, string Name, string? Description, decimal Price, int Amount);