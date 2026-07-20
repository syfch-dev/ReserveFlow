using ReserveFlow.Domain.Users;

namespace ReserveFlow.Application.Abstractions.Authentication;

public interface IJwtTokenProvider
{
    string Generate(Guid userId, string email, IReadOnlyList<RoleName> roles);
}
