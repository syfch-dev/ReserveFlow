using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Shared;

namespace ReserveFlow.Domain.Users;

public interface IUserRepository
{
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);

    void Add(User user);

}
