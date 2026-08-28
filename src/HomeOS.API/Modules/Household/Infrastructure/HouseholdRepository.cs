using HomeOS.API.Modules.Household.Domain;
using Dapper;
using System.Data;

namespace HomeOS.API.Modules.Household.Infrastructure;

public interface IHouseholdRepository
{
    Task<Domain.Household?> FindByIdAsync(Guid id);
    Task<IEnumerable<Domain.Household>> FindByUserIdAsync(Guid userId);
    Task CreateAsync(Domain.Household household);
    Task AddMemberAsync(HouseholdMember member);
    Task<bool> IsMemberAsync(Guid householdId, Guid userId);
    Task CreateInvitationAsync(HouseholdInvitation invitation);
    Task<HouseholdInvitation?> FindInvitationByCodeAsync(string code);
    Task UpdateInvitationStatusAsync(Guid invitationId, string status);
}

public class HouseholdRepository : IHouseholdRepository
{
    private readonly IDbConnection _db;

    public HouseholdRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<Domain.Household?> FindByIdAsync(Guid id)
    {
        const string sql = "SELECT Id, Name, CreatedAt FROM Households WHERE Id = @Id";
        var row = await _db.QuerySingleOrDefaultAsync(sql, new { Id = id });
        return row is null ? null : MapHousehold(row);
    }

    public async Task<IEnumerable<Domain.Household>> FindByUserIdAsync(Guid userId)
    {
        const string sql = """
            SELECT h.Id, h.Name, h.CreatedAt
            FROM Households h
            INNER JOIN HouseholdMembers hm ON hm.HouseholdId = h.Id
            WHERE hm.UserId = @UserId
            """;
        var rows = await _db.QueryAsync(sql, new { UserId = userId });
        return rows.Select(r => (Domain.Household)MapHousehold(r));
    }

    public async Task CreateAsync(Domain.Household household)
    {
        const string sql = "INSERT INTO Households (Id, Name, CreatedAt) VALUES (@Id, @Name, @CreatedAt)";
        await _db.ExecuteAsync(sql, new { household.Id, household.Name, household.CreatedAt });
    }

    public async Task AddMemberAsync(HouseholdMember member)
    {
        const string sql = """
            INSERT INTO HouseholdMembers (HouseholdId, UserId, Role, JoinedAt)
            VALUES (@HouseholdId, @UserId, @Role, @JoinedAt)
            ON CONFLICT (HouseholdId, UserId) DO NOTHING
            """;
        await _db.ExecuteAsync(sql, new { member.HouseholdId, member.UserId, member.Role, member.JoinedAt });
    }

    public async Task<bool> IsMemberAsync(Guid householdId, Guid userId)
    {
        const string sql = "SELECT COUNT(1) FROM HouseholdMembers WHERE HouseholdId = @HouseholdId AND UserId = @UserId";
        var count = await _db.ExecuteScalarAsync<int>(sql, new { HouseholdId = householdId, UserId = userId });
        return count > 0;
    }

    public async Task CreateInvitationAsync(HouseholdInvitation invitation)
    {
        const string sql = """
            INSERT INTO HouseholdInvitations (Id, HouseholdId, Code, ExpiresAt, Status, CreatedAt)
            VALUES (@Id, @HouseholdId, @Code, @ExpiresAt, @Status, @CreatedAt)
            """;
        await _db.ExecuteAsync(sql, new
        {
            invitation.Id,
            invitation.HouseholdId,
            invitation.Code,
            invitation.ExpiresAt,
            invitation.Status,
            invitation.CreatedAt
        });
    }

    public async Task<HouseholdInvitation?> FindInvitationByCodeAsync(string code)
    {
        const string sql = """
            SELECT Id, HouseholdId, Code, ExpiresAt, Status, CreatedAt
            FROM HouseholdInvitations
            WHERE Code = @Code
            """;
        var row = await _db.QuerySingleOrDefaultAsync(sql, new { Code = code.ToUpperInvariant() });
        return row is null ? null : MapInvitation(row);
    }

    public async Task UpdateInvitationStatusAsync(Guid invitationId, string status)
    {
        const string sql = "UPDATE HouseholdInvitations SET Status = @Status WHERE Id = @Id";
        await _db.ExecuteAsync(sql, new { Id = invitationId, Status = status });
    }

    private static Domain.Household MapHousehold(dynamic row)
    {
        var h = (Domain.Household)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(Domain.Household));
        typeof(Domain.Household).GetProperty(nameof(Domain.Household.Id))!.SetValue(h, (Guid)row.id);
        typeof(Domain.Household).GetProperty(nameof(Domain.Household.Name))!.SetValue(h, (string)row.name);
        typeof(Domain.Household).GetProperty(nameof(Domain.Household.CreatedAt))!.SetValue(h, (DateTime)row.createdat);
        return h;
    }

    private static HouseholdInvitation MapInvitation(dynamic row)
    {
        var inv = (HouseholdInvitation)System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(typeof(HouseholdInvitation));
        typeof(HouseholdInvitation).GetProperty(nameof(HouseholdInvitation.Id))!.SetValue(inv, (Guid)row.id);
        typeof(HouseholdInvitation).GetProperty(nameof(HouseholdInvitation.HouseholdId))!.SetValue(inv, (Guid)row.householdid);
        typeof(HouseholdInvitation).GetProperty(nameof(HouseholdInvitation.Code))!.SetValue(inv, (string)row.code);
        typeof(HouseholdInvitation).GetProperty(nameof(HouseholdInvitation.ExpiresAt))!.SetValue(inv, (DateTime)row.expiresat);
        typeof(HouseholdInvitation).GetProperty(nameof(HouseholdInvitation.Status))!.SetValue(inv, (string)row.status);
        typeof(HouseholdInvitation).GetProperty(nameof(HouseholdInvitation.CreatedAt))!.SetValue(inv, (DateTime)row.createdat);
        return inv;
    }
}
