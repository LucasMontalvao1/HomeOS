using HomeOS.API.Modules.Household.Application;
using HomeOS.API.Modules.Household.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HomeOS.API.Modules.Household.Endpoints;

public static class HouseholdEndpoints
{
    public static void MapHouseholdEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/households")
            .WithTags("Household")
            .RequireAuthorization();

        group.MapPost("/", async (
            [FromBody] CreateHouseholdRequest request,
            HouseholdService householdService,
            ClaimsPrincipal user) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Nome da casa é obrigatório." });

            var userId = GetUserId(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            try
            {
                var household = await householdService.CreateAsync(request.Name, userId);
                return Results.Created($"/api/households/{household.Id}", household);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("CreateHousehold")
        .WithSummary("Cria uma nova casa e adiciona o criador como Owner");

        group.MapGet("/", async (
            HouseholdService householdService,
            ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            var households = await householdService.GetUserHouseholdsAsync(userId);
            return Results.Ok(households);
        })
        .WithName("GetMyHouseholds")
        .WithSummary("Lista todas as casas do usuário autenticado");

        group.MapPost("/{householdId:guid}/invite", async (
            Guid householdId,
            HouseholdService householdService,
            ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            try
            {
                var invite = await householdService.GenerateInviteAsync(householdId, userId);
                return Results.Ok(invite);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
        })
        .WithName("GenerateInvite")
        .WithSummary("Gera um código de convite válido por 48h");

        group.MapPost("/join", async (
            [FromBody] JoinHouseholdRequest request,
            HouseholdService householdService,
            ClaimsPrincipal user) =>
        {
            var userId = GetUserId(user);
            if (userId == Guid.Empty) return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(request.Code))
                return Results.BadRequest(new { error = "Código de convite é obrigatório." });

            try
            {
                await householdService.JoinAsync(request.Code, userId);
                return Results.Ok(new { message = "Você entrou na casa com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        })
        .WithName("JoinHousehold")
        .WithSummary("Entra em uma casa usando código de convite");
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)
                  ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }
}
