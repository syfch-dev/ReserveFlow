using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Shared;

namespace ReserveFlow.Domain.Users;

public sealed class User : AggregateRoot
{
    private List<RoleName> _roles = [];

    private User(
        Guid id,
        Email email,
        string passwordHash,
        UserStatus status,
        DateTime createdAtUtc)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        Status = status;
        CreatedAtUtc = createdAtUtc;
    }

    private User()
    {
    }

    public Email Email { get; private set; } = null!;

    public string PasswordHash { get; private set; } = null!;

    public UserStatus Status { get; private set; }

    public IReadOnlyList<RoleName> Roles => _roles;

    public DateTime CreatedAtUtc { get; private set; }

    public static User Register(Email email, string passwordHash, DateTime createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        }

        var user = new User(
            Guid.NewGuid(),
            email,
            passwordHash,
            UserStatus.Active,
            createdAtUtc);

        user._roles.Add(RoleName.Customer);
        user.RaiseDomainEvent(new UserRegisteredDomainEvent(user.Id, user.Email.Value, createdAtUtc));

        return user;
    }
}
