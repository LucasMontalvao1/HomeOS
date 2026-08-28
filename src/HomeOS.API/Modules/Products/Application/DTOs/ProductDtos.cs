namespace HomeOS.API.Modules.Products.Application.DTOs;

public record CreateProductRequest(
    string Name,
    string? Brand,
    string? Category,
    string? Unit,
    string? Barcode);

public record UpdateProductRequest(
    string Name,
    string? Brand,
    string? Category,
    string? Unit,
    string? Barcode);

public record ProductResponse(
    Guid Id,
    Guid HouseholdId,
    string Name,
    string? Brand,
    string? Category,
    string? Unit,
    string? Barcode,
    DateTime CreatedAt,
    DateTime UpdatedAt);
