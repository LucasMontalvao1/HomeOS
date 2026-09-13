using HomeOS.API.Modules.Shopping.Application;
using HomeOS.API.Modules.Shopping.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.API.Modules.Shopping.Endpoints;

public static class ShoppingListEndpoints
{
    public static void MapShoppingListEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/shopping-lists")
            .WithTags("ShoppingLists")
            .RequireAuthorization();

        group.MapGet("/", async (
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            var lists = await shoppingListService.GetAllAsync(householdId);
            return Results.Ok(lists);
        })
        .WithName("GetShoppingLists")
        .WithSummary("Lista todas as listas de compras da casa");

        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            var list = await shoppingListService.GetByIdAsync(id, householdId);
            return list is null ? Results.NotFound() : Results.Ok(list);
        })
        .WithName("GetShoppingListById")
        .WithSummary("Busca uma lista de compras pelo Id");

        group.MapPost("/", async (
            [FromBody] CreateShoppingListRequest request,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });
            
            try
            {
                var list = await shoppingListService.CreateAsync(request, householdId);
                return Results.Created($"/api/shopping-lists/{list.Id}", list);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateShoppingList")
        .WithSummary("Cria uma nova lista de compras");

        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateShoppingListRequest request,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var list = await shoppingListService.UpdateAsync(id, request, householdId);
                return Results.Ok(list);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("UpdateShoppingList")
        .WithSummary("Atualiza o nome da lista de compras");

        group.MapDelete("/{id:guid}", async (
            Guid id,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                await shoppingListService.DeleteAsync(id, householdId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteShoppingList")
        .WithSummary("Exclui uma lista de compras");

        // ─── Status Actions ────────────────────────────────────────────────────────
        
        group.MapPost("/{id:guid}/start", async (
            Guid id,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var list = await shoppingListService.StartShoppingAsync(id, householdId);
                return Results.Ok(list);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("StartShoppingList");

        group.MapPost("/{id:guid}/complete", async (
            Guid id,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var list = await shoppingListService.CompleteShoppingAsync(id, householdId);
                return Results.Ok(list);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("CompleteShoppingList");

        // ─── Items Actions ─────────────────────────────────────────────────────────

        group.MapPost("/{id:guid}/items", async (
            Guid id,
            [FromBody] AddItemRequest request,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var item = await shoppingListService.AddItemAsync(id, request, householdId);
                return Results.Created($"/api/shopping-lists/{id}/items/{item.Id}", item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("AddShoppingListItem");

        group.MapDelete("/{listId:guid}/items/{itemId:guid}", async (
            Guid listId,
            Guid itemId,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                await shoppingListService.RemoveItemAsync(listId, itemId, householdId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("RemoveShoppingListItem");

        group.MapPut("/{listId:guid}/items/{itemId:guid}/check", async (
            Guid listId,
            Guid itemId,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var item = await shoppingListService.CheckItemAsync(listId, itemId, householdId);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).WithName("CheckShoppingListItem");

        group.MapPut("/{listId:guid}/items/{itemId:guid}/uncheck", async (
            Guid listId,
            Guid itemId,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var item = await shoppingListService.UncheckItemAsync(listId, itemId, householdId);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        }).WithName("UncheckShoppingListItem");
        
        group.MapPut("/{listId:guid}/items/{itemId:guid}/details", async (
            Guid listId,
            Guid itemId,
            [FromBody] UpdateItemDetailsRequest request,
            [FromQuery] Guid householdId,
            ShoppingListService shoppingListService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                var item = await shoppingListService.UpdateItemDetailsAsync(listId, itemId, request, householdId);
                return Results.Ok(item);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        }).WithName("UpdateShoppingListItemDetails");
    }

    private static bool IsValidHousehold(Guid id) => id != Guid.Empty;
}
