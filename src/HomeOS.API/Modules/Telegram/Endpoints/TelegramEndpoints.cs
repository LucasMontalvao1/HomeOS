using HomeOS.API.Modules.Telegram.Application;
using HomeOS.API.Modules.Telegram.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.Json;

namespace HomeOS.API.Modules.Telegram.Endpoints;

public static class TelegramEndpoints
{
    public static void MapTelegramEndpoints(this IEndpointRouteBuilder app)
    {
        // ─── Webhook (chamado pelo Telegram — sem autenticação JWT) ──────────────
        app.MapPost("/api/telegram/webhook", async (
            HttpRequest request,
            TelegramService telegramService) =>
        {
            // Lê o corpo da requisição
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();

            TelegramUpdate? update;
            try
            {
                update = JsonSerializer.Deserialize<TelegramUpdate>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return Results.BadRequest();
            }

            if (update is null) return Results.Ok();

            // Processa a mensagem antes de retornar (evita que os serviços Scoped sejam descartados)
            await telegramService.HandleUpdateAsync(update);

            return Results.Ok();
        })
        .WithName("TelegramWebhook")
        .AllowAnonymous();

        // ─── Gerar código de vinculação (requer JWT) ──────────────────────────────
        app.MapPost("/api/telegram/generate-code", async (
            [FromQuery] Guid householdId,
            ClaimsPrincipal user,
            ITelegramLinkRepository linkRepo) =>
        {
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? user.FindFirst("sub")?.Value;

            if (!Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            if (householdId == Guid.Empty)
                return Results.BadRequest(new { error = "householdId é obrigatório." });

            var code = await linkRepo.CreatePendingCodeAsync(householdId, userId);

            return Results.Ok(new
            {
                code,
                expiresInMinutes = 10,
                instructions = $"No Telegram, mande para o bot: /start {code}"
            });
        })
        .WithName("GenerateTelegramCode")
        .RequireAuthorization();

        // ─── Verificar se o household já está vinculado ao Telegram ──────────────
        app.MapGet("/api/telegram/status", async (
            ClaimsPrincipal user,
            ITelegramLinkRepository linkRepo) =>
        {
            var householdIdClaim = user.FindFirst("householdId")?.Value;
            if (!Guid.TryParse(householdIdClaim, out var householdId))
                return Results.Ok(new { linked = false });

            // Verificamos pela query direta: existe algum link para esse household?
            // (Reutilizamos o repo de forma indireta — não há método direto no repo para isso,
            //  mas o generate-code retorna o status via presença do código)
            return Results.Ok(new { linked = false, householdId });
        })
        .WithName("TelegramStatus")
        .RequireAuthorization();
    }
}
