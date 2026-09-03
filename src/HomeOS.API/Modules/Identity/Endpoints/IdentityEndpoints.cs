using HomeOS.API.Modules.Identity.Application;
using HomeOS.API.Modules.Identity.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace HomeOS.API.Modules.Identity.Endpoints;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identity").WithTags("Identity");

        group.MapPost("/register", async (
            [FromBody] RegisterRequest request,
            AuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Nome é obrigatório." });
            if (string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest(new { error = "Email é obrigatório." });
            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { error = "Senha é obrigatória." });
            } 
            try
            {
                var response = await authService.RegisterAsync(request);
                return Results.Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        })
        .WithName("Register")
        .WithSummary("Registra um novo usuário")
        .AllowAnonymous();

        group.MapPost("/login", async (
            [FromBody] LoginRequest request,
            AuthService authService) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return Results.BadRequest(new { error = "Email e senha são obrigatórios." });

            try
            {
                var response = await authService.LoginAsync(request);
                return Results.Ok(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("Login")
        .WithSummary("Realiza login e retorna token JWT")
        .AllowAnonymous();
    }
}
