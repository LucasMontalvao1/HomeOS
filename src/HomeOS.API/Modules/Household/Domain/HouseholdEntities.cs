namespace HomeOS.API.Modules.Household.Domain;

public class Household
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Household()
    {
        Name = string.Empty;
    }

    public static Household Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Nome da casa é obrigatório.", nameof(name));

        return new Household
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            CreatedAt = DateTime.UtcNow
        };
    }
}

public class HouseholdMember
{
    public Guid HouseholdId { get; private set; }
    public Guid UserId { get; private set; }
    public string Role { get; private set; }
    public DateTime JoinedAt { get; private set; }

    private HouseholdMember()
    {
        Role = string.Empty;
    }

    public static HouseholdMember CreateOwner(Guid householdId, Guid userId)
        => new() { HouseholdId = householdId, UserId = userId, Role = "Owner", JoinedAt = DateTime.UtcNow };

    public static HouseholdMember CreateMember(Guid householdId, Guid userId)
        => new() { HouseholdId = householdId, UserId = userId, Role = "Member", JoinedAt = DateTime.UtcNow };
}

public class HouseholdInvitation
{
    public Guid Id { get; private set; }
    public Guid HouseholdId { get; private set; }
    public string Code { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private HouseholdInvitation()
    {
        Code = string.Empty;
        Status = string.Empty;
    }

    public bool IsValid() => Status == "Pending" && ExpiresAt > DateTime.UtcNow;

    public static HouseholdInvitation Create(Guid householdId, TimeSpan validFor)
    {
        return new HouseholdInvitation
        {
            Id = Guid.NewGuid(),
            HouseholdId = householdId,
            Code = Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
            ExpiresAt = DateTime.UtcNow.Add(validFor),
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
    }
}
