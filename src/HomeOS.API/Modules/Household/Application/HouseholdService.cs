using HomeOS.API.Modules.Household.Application.DTOs;
using HomeOS.API.Modules.Household.Domain;
using HomeOS.API.Modules.Household.Infrastructure;

namespace HomeOS.API.Modules.Household.Application;

public class HouseholdService
{
    private readonly IHouseholdRepository _householdRepository;

    public HouseholdService(IHouseholdRepository householdRepository)
    {
        _householdRepository = householdRepository;
    }

    public async Task<HouseholdResponse> CreateAsync(string name, Guid ownerUserId)
    {
        var household = Domain.Household.Create(name);
        await _householdRepository.CreateAsync(household);

        var owner = HouseholdMember.CreateOwner(household.Id, ownerUserId);
        await _householdRepository.AddMemberAsync(owner);

        return new HouseholdResponse(household.Id, household.Name, household.CreatedAt);
    }

    public async Task<IEnumerable<HouseholdResponse>> GetUserHouseholdsAsync(Guid userId)
    {
        var households = await _householdRepository.FindByUserIdAsync(userId);
        return households.Select(h => new HouseholdResponse(h.Id, h.Name, h.CreatedAt));
    }

    public async Task<InviteResponse> GenerateInviteAsync(Guid householdId, Guid requestingUserId)
    {
        var isMember = await _householdRepository.IsMemberAsync(householdId, requestingUserId);
        if (!isMember)
            throw new UnauthorizedAccessException("Você não é membro desta casa.");

        var invitation = HouseholdInvitation.Create(householdId, TimeSpan.FromHours(48));
        await _householdRepository.CreateInvitationAsync(invitation);

        return new InviteResponse(invitation.Code, invitation.ExpiresAt);
    }

    public async Task JoinAsync(string code, Guid userId)
    {
        var invitation = await _householdRepository.FindInvitationByCodeAsync(code);

        if (invitation is null || !invitation.IsValid())
            throw new InvalidOperationException("Convite inválido ou expirado.");

        var alreadyMember = await _householdRepository.IsMemberAsync(invitation.HouseholdId, userId);
        if (alreadyMember)
            throw new InvalidOperationException("Você já é membro desta casa.");

        var member = HouseholdMember.CreateMember(invitation.HouseholdId, userId);
        await _householdRepository.AddMemberAsync(member);
        await _householdRepository.UpdateInvitationStatusAsync(invitation.Id, "Accepted");
    }
}
