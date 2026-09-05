namespace HomeOS.API.Modules.Products.Application.DTOs;

public record CreateProductRequest(
    string Name,
    string? Brand,
    string? Category,
    string? Unit,
    string? Barcode,
    string? ImageUrl);

public record UpdateProductRequest(
    string Name,
    string? Brand,
    string? Category,
    string? Unit,
    string? Barcode,
    string? ImageUrl);

public record ProductResponse(
    Guid Id,
    Guid HouseholdId,
    string Name,
    string? Brand,
    string? Category,
    string? Unit,
    string? Barcode,
    string? ImageUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt);
