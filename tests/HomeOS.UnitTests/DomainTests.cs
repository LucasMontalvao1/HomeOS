using HomeOS.API.Modules.Identity.Domain;
using HomeOS.API.Modules.Household.Domain;

namespace HomeOS.UnitTests;

public class UserDomainTests
{
    [Fact]
    public void User_Create_ShouldSucceed_WithValidData()
    {
        var user = User.Create("Lucas", "lucas@email.com", "hashed_password");

        Assert.NotEqual(Guid.Empty, user.Id);
        Assert.Equal("Lucas", user.Name);
        Assert.Equal("lucas@email.com", user.Email);
        Assert.Equal("hashed_password", user.PasswordHash);
    }

    [Fact]
    public void User_Create_ShouldNormalize_Email()
    {
        var user = User.Create("Lucas", "  LUCAS@EMAIL.COM  ", "hash");
        Assert.Equal("lucas@email.com", user.Email);
    }

    [Theory]
    [InlineData("", "email@test.com", "hash")]
    [InlineData("Lucas", "", "hash")]
    [InlineData("Lucas", "email@test.com", "")]
    public void User_Create_ShouldThrow_WhenRequiredFieldMissing(string name, string email, string hash)
    {
        Assert.Throws<ArgumentException>(() => User.Create(name, email, hash));
    }
}

public class HouseholdDomainTests
{
    [Fact]
    public void Household_Create_ShouldSucceed_WithValidName()
    {
        var household = Household.Create("Casa dos Lucas");
        Assert.NotEqual(Guid.Empty, household.Id);
        Assert.Equal("Casa dos Lucas", household.Name);
    }

    [Fact]
    public void Household_Create_ShouldThrow_WhenNameEmpty()
    {
        Assert.Throws<ArgumentException>(() => Household.Create(""));
    }

    [Fact]
    public void HouseholdInvitation_IsValid_ShouldReturnTrue_WhenPendingAndNotExpired()
    {
        var invitation = HouseholdInvitation.Create(Guid.NewGuid(), TimeSpan.FromHours(48));
        Assert.True(invitation.IsValid());
    }

    [Fact]
    public void HouseholdInvitation_Create_ShouldGenerateCode_WithCorrectLength()
    {
        var invitation = HouseholdInvitation.Create(Guid.NewGuid(), TimeSpan.FromHours(48));
        Assert.Equal(12, invitation.Code.Length);
        Assert.Equal(invitation.Code, invitation.Code.ToUpperInvariant());
    }

    [Fact]
    public void HouseholdMember_CreateOwner_ShouldSetRoleOwner()
    {
        var member = HouseholdMember.CreateOwner(Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal("Owner", member.Role);
    }

    [Fact]
    public void HouseholdMember_CreateMember_ShouldSetRoleMember()
    {
        var member = HouseholdMember.CreateMember(Guid.NewGuid(), Guid.NewGuid());
        Assert.Equal("Member", member.Role);
    }
}
