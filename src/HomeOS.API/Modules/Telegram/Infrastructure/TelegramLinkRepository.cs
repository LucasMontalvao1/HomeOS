using Dapper;
using System.Data;

namespace HomeOS.API.Modules.Telegram.Infrastructure;

public interface ITelegramLinkRepository
{
    Task<Guid?> FindHouseholdByChatIdAsync(long chatId);
    Task<Guid?> FindHouseholdByCodeAsync(string code);
    Task CreateLinkAsync(long chatId, Guid householdId, Guid linkedByUserId);
    Task<string> CreatePendingCodeAsync(Guid householdId, Guid userId);
    Task DeletePendingCodeAsync(string code);

    Task<Guid?> GetActiveShoppingListAsync(long chatId);
    Task SetActiveShoppingListAsync(long chatId, Guid? listId);
}

public class TelegramLinkRepository : ITelegramLinkRepository
{
    private readonly IDbConnection _db;

    public TelegramLinkRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Guid?> FindHouseholdByChatIdAsync(long chatId)
    {
        const string sql = """
            SELECT HouseholdId FROM TelegramLinks
            WHERE ChatId = @ChatId
            """;
        var result = await _db.QuerySingleOrDefaultAsync<Guid?>(sql, new { ChatId = chatId });
        return result;
    }

    public async Task<Guid?> FindHouseholdByCodeAsync(string code)
    {
        const string sql = """
            SELECT HouseholdId FROM TelegramPendingLinks
            WHERE Code = @Code AND ExpiresAt > NOW()
            """;
        var result = await _db.QuerySingleOrDefaultAsync<Guid?>(sql, new { Code = code.ToUpper() });
        return result;
    }

    public async Task CreateLinkAsync(long chatId, Guid householdId, Guid linkedByUserId)
    {
        const string sql = """
            INSERT INTO TelegramLinks (HouseholdId, ChatId, LinkedByUserId, LinkedAt)
            VALUES (@HouseholdId, @ChatId, @LinkedByUserId, NOW())
            ON CONFLICT (ChatId) DO UPDATE SET HouseholdId = @HouseholdId, LinkedByUserId = @LinkedByUserId, LinkedAt = NOW()
            """;
        await _db.ExecuteAsync(sql, new { ChatId = chatId, HouseholdId = householdId, LinkedByUserId = linkedByUserId });
    }

    public async Task<string> CreatePendingCodeAsync(Guid householdId, Guid userId)
    {
        // Gera código único de 6 caracteres alfanuméricos
        var code = GenerateCode();

        // Remove códigos antigos do mesmo household
        const string deleteSql = "DELETE FROM TelegramPendingLinks WHERE HouseholdId = @HouseholdId";
        await _db.ExecuteAsync(deleteSql, new { HouseholdId = householdId });

        const string insertSql = """
            INSERT INTO TelegramPendingLinks (Code, HouseholdId, CreatedByUserId, ExpiresAt)
            VALUES (@Code, @HouseholdId, @UserId, NOW() + INTERVAL '10 minutes')
            """;
        await _db.ExecuteAsync(insertSql, new { Code = code, HouseholdId = householdId, UserId = userId });

        return code;
    }

    public async Task DeletePendingCodeAsync(string code)
    {
        const string sql = "DELETE FROM TelegramPendingLinks WHERE Code = @Code";
        await _db.ExecuteAsync(sql, new { Code = code.ToUpper() });
    }

    public async Task<Guid?> GetActiveShoppingListAsync(long chatId)
    {
        const string sql = "SELECT ActiveShoppingListId FROM TelegramSessions WHERE ChatId = @ChatId";
        return await _db.QuerySingleOrDefaultAsync<Guid?>(sql, new { ChatId = chatId });
    }

    public async Task SetActiveShoppingListAsync(long chatId, Guid? listId)
    {
        const string sql = """
            INSERT INTO TelegramSessions (ChatId, ActiveShoppingListId, UpdatedAt)
            VALUES (@ChatId, @ListId, NOW())
            ON CONFLICT (ChatId) DO UPDATE SET ActiveShoppingListId = @ListId, UpdatedAt = NOW()
            """;
        await _db.ExecuteAsync(sql, new { ChatId = chatId, ListId = listId });
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // sem I, O, 0, 1 para evitar confusão
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
