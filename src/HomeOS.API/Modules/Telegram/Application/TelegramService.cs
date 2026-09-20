using HomeOS.API.Modules.Products.Infrastructure;
using HomeOS.API.Modules.Shopping.Application;
using HomeOS.API.Modules.Shopping.Application.DTOs;
using HomeOS.API.Modules.Telegram.Configuration;
using HomeOS.API.Modules.Telegram.Infrastructure;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace HomeOS.API.Modules.Telegram.Application;

public class TelegramService
{
    private readonly ITelegramLinkRepository _linkRepo;
    private readonly ShoppingListService _shoppingListService;
    private readonly IProductRepository _productRepository;
    private readonly TelegramSettings _settings;
    private readonly HttpClient _http;
    private readonly ILogger<TelegramService> _logger;

    public TelegramService(
        ITelegramLinkRepository linkRepo,
        ShoppingListService shoppingListService,
        IProductRepository productRepository,
        IOptions<TelegramSettings> settings,
        HttpClient http,
        ILogger<TelegramService> logger)
    {
        _linkRepo = linkRepo;
        _shoppingListService = shoppingListService;
        _productRepository = productRepository;
        _settings = settings.Value;
        _http = http;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(TelegramUpdate update)
    {
        if (update.Message is null) return;

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text?.Trim() ?? string.Empty;

        _logger.LogInformation("[Telegram] Mensagem recebida de chatId={ChatId}: {Text}", chatId, text);

        if (string.IsNullOrWhiteSpace(text)) return;

        // Verifica se o chat está vinculado
        var householdId = await _linkRepo.FindHouseholdByChatIdAsync(chatId);

        // Comando /start [codigo] — vincula a conta mesmo sem householdId
        if (text.StartsWith("/start"))
        {
            await HandleStartAsync(chatId, text, householdId);
            return;
        }

        if (householdId is null)
        {
            await SendMessageAsync(chatId,
                "❌ Sua conta ainda não está vinculada ao HomeOS.\n\n" +
                "Abra o HomeOS no navegador, vá em *Configurações* e clique em *Conectar Telegram* para obter seu código de vinculação.\n\n" +
                "Depois mande: `/start SEUCODIGO`",
                parseMode: "Markdown");
            return;
        }

        var hid = householdId.Value;

        if (text.StartsWith("/listas")) await HandleListasAsync(chatId, hid);
        else if (text.StartsWith("/criar")) await HandleCriarAsync(chatId, text, hid);
        else if (text.StartsWith("/abrir")) await HandleAbrirAsync(chatId, text, hid);
        else if (text.StartsWith("/resumo")) await HandleResumoAsync(chatId, hid);
        else if (text.StartsWith("/add")) await HandleAddAsync(chatId, text, hid);
        else if (text.StartsWith("/ajuda") || text.StartsWith("/help")) await HandleAjudaAsync(chatId);
        else
        {
            await SendMessageAsync(chatId,
                "Não entendi o comando. Use /ajuda para ver os comandos disponíveis.");
        }
    }

    // ─── /start [codigo] ──────────────────────────────────────────────────────

    private async Task HandleStartAsync(long chatId, string text, Guid? householdId)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2)
        {
            if (householdId.HasValue)
            {
                await SendMessageAsync(chatId,
                    "✅ Sua conta já está vinculada ao HomeOS! Use /ajuda para ver os comandos.");
            }
            else
            {
                await SendMessageAsync(chatId,
                    "Olá! Eu sou o bot do *HomeOS* 🏠\n\n" +
                    "Para começar, abra o HomeOS, vá em *Configurações* → *Conectar Telegram* e use o código gerado.\n\n" +
                    "Depois mande `/start SEUCODIGO`",
                    parseMode: "Markdown");
            }
            return;
        }

        var code = parts[1].ToUpper();
        var linkedHouseholdId = await _linkRepo.FindHouseholdByCodeAsync(code);

        if (linkedHouseholdId is null)
        {
            await SendMessageAsync(chatId,
                "❌ Código inválido ou expirado. Gere um novo código no HomeOS e tente novamente.");
            return;
        }

        // Vincula o chat ao household — usamos um userId genérico aqui pois o Telegram não tem JWT
        await _linkRepo.CreateLinkAsync(chatId, linkedHouseholdId.Value, linkedHouseholdId.Value);
        await _linkRepo.DeletePendingCodeAsync(code);

        await SendMessageAsync(chatId,
            "✅ *Conta vinculada com sucesso!*\n\n" +
            "Agora você pode gerenciar suas listas de compras pelo Telegram.\n\n" +
            "Use /ajuda para ver os comandos disponíveis 🛒",
            parseMode: "Markdown");
    }

    // ─── /listas ─────────────────────────────────────────────────────────────

    private async Task HandleListasAsync(long chatId, Guid householdId)
    {
        var lists = (await _shoppingListService.GetAllAsync(householdId)).ToList();

        if (!lists.Any())
        {
            await SendMessageAsync(chatId, "📋 Você não tem nenhuma lista de compras ainda.\n\nCrie uma com: /criar Nome da Lista");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("🛒 *Suas listas de compras:*\n");

        var statusEmoji = new Dictionary<string, string>
        {
            ["Pending"] = "⏳",
            ["InProgress"] = "🛍️",
            ["Completed"] = "✅",
            ["Cancelled"] = "❌"
        };

        foreach (var list in lists)
        {
            var emoji = statusEmoji.GetValueOrDefault(list.Status, "📋");
            sb.AppendLine($"{emoji} *{list.Name}*");
            sb.AppendLine($"   {list.CheckedItems}/{list.TotalItems} itens marcados");
        }

        sb.AppendLine("\nPara abrir uma lista: /abrir Nome da Lista");

        await SendMessageAsync(chatId, sb.ToString(), parseMode: "Markdown");
    }

    // ─── /criar [nome] ────────────────────────────────────────────────────────

    private async Task HandleCriarAsync(long chatId, string text, Guid householdId)
    {
        var name = text.Replace("/criar", "").Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            await SendMessageAsync(chatId, "Por favor, informe o nome da lista. Ex: /criar Compras de Outubro");
            return;
        }

        var list = await _shoppingListService.CreateAsync(new CreateShoppingListRequest(name), householdId);
        await _linkRepo.SetActiveShoppingListAsync(chatId, list.Id);

        await SendMessageAsync(chatId,
            $"✅ Lista *{list.Name}* criada e selecionada!\n\n" +
            "Adicione itens com: /add produto\nEx: /add arroz 5kg",
            parseMode: "Markdown");
    }

    // ─── /abrir [nome] ────────────────────────────────────────────────────────

    private async Task HandleAbrirAsync(long chatId, string text, Guid householdId)
    {
        var name = text.Replace("/abrir", "").Trim().ToLower();
        if (string.IsNullOrWhiteSpace(name))
        {
            await SendMessageAsync(chatId, "Informe o nome da lista. Ex: /abrir Compras de Outubro");
            return;
        }

        var lists = await _shoppingListService.GetAllAsync(householdId);
        var match = lists.FirstOrDefault(l => l.Name.ToLower().Contains(name));

        if (match is null)
        {
            await SendMessageAsync(chatId, $"❌ Nenhuma lista encontrada com o nome \"{name}\".\n\nUse /listas para ver todas.");
            return;
        }

        await _linkRepo.SetActiveShoppingListAsync(chatId, match.Id);
        await SendMessageAsync(chatId,
            $"✅ Lista *{match.Name}* selecionada!\n\n" +
            "Use /resumo para ver os itens ou /add para adicionar.",
            parseMode: "Markdown");
    }

    // ─── /resumo ─────────────────────────────────────────────────────────────

    private async Task HandleResumoAsync(long chatId, Guid householdId)
    {
        var listId = await _linkRepo.GetActiveShoppingListAsync(chatId);
        if (listId is null)
        {
            await SendMessageAsync(chatId, "Nenhuma lista selecionada. Use /listas e depois /abrir Nome da Lista");
            return;
        }

        var list = await _shoppingListService.GetByIdAsync(listId.Value, householdId);
        if (list is null)
        {
            await SendMessageAsync(chatId, "Lista não encontrada.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"🛒 *{list.Name}*\n");

        if (!list.Items.Any())
        {
            sb.AppendLine("_Nenhum item adicionado ainda._");
            sb.AppendLine("\nUse /add produto para adicionar itens.");
        }
        else
        {
            foreach (var item in list.Items)
            {
                var check = item.Checked ? "✅" : "⬜";
                var price = item.Price.HasValue ? $" — R$ {item.Price:N2}" : "";
                sb.AppendLine($"{check} {item.ProductName} ({item.Quantity} {item.Unit ?? "un"}){price}");
            }

            sb.AppendLine($"\n_{list.Items.Count(i => i.Checked)}/{list.Items.Count()} marcados_");
        }

        await SendMessageAsync(chatId, sb.ToString(), parseMode: "Markdown");
    }

    // ─── /add [produto] ───────────────────────────────────────────────────────

    private async Task HandleAddAsync(long chatId, string text, Guid householdId)
    {
        var listId = await _linkRepo.GetActiveShoppingListAsync(chatId);
        if (listId is null)
        {
            await SendMessageAsync(chatId,
                "Nenhuma lista selecionada. Use /listas para ver suas listas e /abrir para selecionar uma.");
            return;
        }

        var productName = text.Replace("/add", "").Trim();
        if (string.IsNullOrWhiteSpace(productName))
        {
            await SendMessageAsync(chatId, "Informe o produto. Ex: /add arroz 5kg");
            return;
        }

        // Busca o produto pelo nome no catálogo
        var products = await SearchProductsAsync(productName, householdId);

        if (!products.Any())
        {
            await SendMessageAsync(chatId,
                $"❌ Produto *{productName}* não encontrado no catálogo.\n\n" +
                "Cadastre produtos primeiro no HomeOS Web.",
                parseMode: "Markdown");
            return;
        }

        // Usa o primeiro resultado encontrado
        var product = products.First();

        try
        {
            await _shoppingListService.AddItemAsync(listId.Value,
                new AddItemRequest(product.Id, 1, product.Unit),
                householdId);

            await SendMessageAsync(chatId,
                $"✅ *{product.Name}* adicionado à lista!\n\n" +
                "Use /resumo para ver todos os itens.",
                parseMode: "Markdown");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("já existe"))
        {
            await SendMessageAsync(chatId, $"ℹ️ {product.Name} já está na lista.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Telegram] Erro ao adicionar item");
            await SendMessageAsync(chatId, "❌ Erro ao adicionar o item. Tente novamente.");
        }
    }

    // ─── /ajuda ──────────────────────────────────────────────────────────────

    private async Task HandleAjudaAsync(long chatId)
    {
        const string msg = """
            🏠 *HomeOS Bot — Comandos disponíveis*

            /listas — Ver todas as suas listas de compras
            /criar [nome] — Criar uma nova lista
            /abrir [nome] — Selecionar uma lista como ativa
            /resumo — Ver os itens da lista selecionada
            /add [produto] — Adicionar produto à lista selecionada
            /ajuda — Mostrar esta mensagem

            _Dica: selecione uma lista com /abrir antes de adicionar itens._
            """;
        await SendMessageAsync(chatId, msg, parseMode: "Markdown");
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    public async Task SendMessageAsync(long chatId, string text, string? parseMode = null)
    {
        try
        {
            var payload = new Dictionary<string, object>
            {
                ["chat_id"] = chatId,
                ["text"] = text
            };
            if (parseMode != null) payload["parse_mode"] = parseMode;

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync($"{_settings.ApiBaseUrl}/sendMessage", content);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("[Telegram] Falha ao enviar mensagem: {Body}", body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Telegram] Erro ao enviar mensagem para chatId={ChatId}", chatId);
        }
    }

    private async Task<List<HomeOS.API.Modules.Products.Domain.Product>> SearchProductsAsync(string query, Guid householdId)
    {
        var all = await _productRepository.GetAllAsync(householdId);
        return all
            .Where(p => p.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || (p.Brand != null && p.Brand.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }
}

// ─── Models do payload do Telegram ───────────────────────────────────────────

public record TelegramUpdate(
    [property: System.Text.Json.Serialization.JsonPropertyName("update_id")] long UpdateId,
    [property: System.Text.Json.Serialization.JsonPropertyName("message")] TelegramMessage? Message
);

public record TelegramMessage(
    [property: System.Text.Json.Serialization.JsonPropertyName("message_id")] long MessageId,
    [property: System.Text.Json.Serialization.JsonPropertyName("text")] string? Text,
    [property: System.Text.Json.Serialization.JsonPropertyName("chat")] TelegramChat Chat
);

public record TelegramChat(
    [property: System.Text.Json.Serialization.JsonPropertyName("id")] long Id
);
