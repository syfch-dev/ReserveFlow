using ReserveFlow.Domain.Shared;
using ReserveFlow.Domain.Users;

namespace ReserveFlow.Booking.Tests;

public class UserTests
{
    [Fact]
    public void Email_Create_ShouldNormalizeAndValidate()
    {
        var email = Email.Create("  Admin@Example.COM ");

        Assert.Equal("admin@example.com", email.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("@missing-local.com")]
    public void Email_Create_ShouldRejectInvalidValues(string value)
    {
        Assert.Throws<ArgumentException>(() => Email.Create(value));
    }

    [Fact]
    public void User_Register_ShouldCreateActiveCustomerWithHash()
    {
        var email = Email.Create("customer@example.com");
        var createdAt = new DateTime(2026, 7, 12, 12, 0, 0, DateTimeKind.Utc);

        var user = User.Register(email, "hashed-password", createdAt);

        Assert.Equal(UserStatus.Active, user.Status);
        Assert.Equal("hashed-password", user.PasswordHash);
        Assert.Equal(createdAt, user.CreatedAtUtc);
        Assert.Contains(RoleName.Customer, user.Roles);
        Assert.Single(user.GetDomainEvents());
        Assert.IsType<UserRegisteredDomainEvent>(user.GetDomainEvents()[0]);
    }

    [Fact]
    public void User_Register_ShouldRejectMissingPasswordHash()
    {
        var email = Email.Create("customer@example.com");

        Assert.Throws<ArgumentException>(() =>
            User.Register(email, " ", DateTime.UtcNow));
    }
}
