using Microsoft.AspNetCore.Identity;
using ReserveFlow.Application.Abstractions.Authentication;

namespace ReserveFlow.Infrastructure.Authentication;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(new object(), password);

    public bool Verify(string password, string passwordHash) =>
        _hasher.VerifyHashedPassword(new object(), passwordHash, password)
            is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
}
