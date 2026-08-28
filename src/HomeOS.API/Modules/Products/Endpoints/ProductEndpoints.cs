using HomeOS.API.Modules.Products.Application;
using HomeOS.API.Modules.Products.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeOS.API.Modules.Products.Endpoints;

public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/products")
            .WithTags("Products")
            .RequireAuthorization();

        // GET /api/products?q=arroz
        group.MapGet("/", async (
            [FromQuery] string? q,
            [FromQuery] Guid householdId,
            ProductService productService,
            ClaimsPrincipal user) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            var products = string.IsNullOrWhiteSpace(q)
                ? await productService.GetAllAsync(householdId)
                : await productService.SearchAsync(q, householdId);

            return Results.Ok(products);
        })
        .WithName("GetProducts")
        .WithSummary("Lista ou busca produtos da casa");

        // GET /api/products/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromQuery] Guid householdId,
            ProductService productService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            var product = await productService.GetByIdAsync(id, householdId);
            return product is null ? Results.NotFound() : Results.Ok(product);
        })
        .WithName("GetProductById")
        .WithSummary("Busca produto por Id");

        // POST /api/products
        group.MapPost("/", async (
            [FromBody] CreateProductRequest request,
            [FromQuery] Guid householdId,
            ProductService productService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Nome do produto é obrigatório." });

            try
            {
                var product = await productService.CreateAsync(request, householdId);
                return Results.Created($"/api/products/{product.Id}", product);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateProduct")
        .WithSummary("Cria um novo produto no catálogo da casa");

        // PUT /api/products/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateProductRequest request,
            [FromQuery] Guid householdId,
            ProductService productService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Nome do produto é obrigatório." });

            try
            {
                var product = await productService.UpdateAsync(id, request, householdId);
                return Results.Ok(product);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("UpdateProduct")
        .WithSummary("Atualiza um produto existente");

        // DELETE /api/products/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            [FromQuery] Guid householdId,
            ProductService productService) =>
        {
            if (!IsValidHousehold(householdId)) return Results.BadRequest(new { error = "householdId é obrigatório." });

            try
            {
                await productService.DeleteAsync(id, householdId);
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
        .WithName("DeleteProduct")
        .WithSummary("Remove um produto do catálogo da casa");
    }

    private static bool IsValidHousehold(Guid id) => id != Guid.Empty;
}
