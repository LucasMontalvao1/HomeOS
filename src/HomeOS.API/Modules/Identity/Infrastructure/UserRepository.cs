using HomeOS.API.Modules.Identity.Domain;
using HomeOS.API.Modules.Identity.Infrastructure;
using System.Data;
using Dapper;

namespace HomeOS.API.Modules.Identity.Infrastructure;

public interface IUserRepository
{
    Task<User?> FindByEmailAsync(string email);
    Task CreateAsync(User user);
}

public class UserRepository : IUserRepository
{
    private readonly IDbConnection _db;

    public UserRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        const string sql = """
            SELECT Id, Name, Email, PasswordHash, CreatedAt
            FROM Users
            WHERE Email = @Email
            """;

        var row = await _db.QuerySingleOrDefaultAsync(sql, new { Email = email.ToLowerInvariant() });
        if (row is null) return null;

        return MapToDomain(row);
    }

    public async Task CreateAsync(User user)
    {
        const string sql = """
            INSERT INTO Users (Id, Name, Email, PasswordHash, CreatedAt)
            VALUES (@Id, @Name, @Email, @PasswordHash, @CreatedAt)
            """;

        await _db.ExecuteAsync(sql, new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PasswordHash,
            user.CreatedAt
        });
    }

    private static User MapToDomain(dynamic row)
    {
        // Reflection-based mapping to preserve encapsulation
        var user = (User)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(User));
        typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, (Guid)row.id);
        typeof(User).GetProperty(nameof(User.Name))!.SetValue(user, (string)row.name);
        typeof(User).GetProperty(nameof(User.Email))!.SetValue(user, (string)row.email);
        typeof(User).GetProperty(nameof(User.PasswordHash))!.SetValue(user, (string)row.passwordhash);
        typeof(User).GetProperty(nameof(User.CreatedAt))!.SetValue(user, (DateTime)row.createdat);
        return user;
    }
}
