namespace HomeOS.API.Modules.Household.Application.DTOs;

public record CreateHouseholdRequest(string Name);

public record HouseholdResponse(Guid Id, string Name, DateTime CreatedAt);

public record InviteResponse(string Code, DateTime ExpiresAt);

public record JoinHouseholdRequest(string Code);
