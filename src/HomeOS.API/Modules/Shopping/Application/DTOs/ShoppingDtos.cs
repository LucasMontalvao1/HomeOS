using HomeOS.API.Modules.Shopping.Domain;

namespace HomeOS.API.Modules.Shopping.Application.DTOs;

public record CreateShoppingListRequest(string Name);

public record UpdateShoppingListRequest(string Name);

public record AddItemRequest(Guid ProductId, decimal Quantity, string? Unit);

public record UpdateItemQuantityRequest(decimal Quantity);

public record ShoppingListItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string? ProductBrand,
    string? ProductBarcode,
    decimal Quantity,
    string? Unit,
    bool Checked,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ShoppingListResponse(
    Guid Id,
    Guid HouseholdId,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    IEnumerable<ShoppingListItemResponse> Items);

public record ShoppingListSummaryResponse(
    Guid Id,
    Guid HouseholdId,
    string Name,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    int TotalItems,
    int CheckedItems);
