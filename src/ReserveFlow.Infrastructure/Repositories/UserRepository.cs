using Microsoft.EntityFrameworkCore;
using ReserveFlow.Domain.Shared;
using ReserveFlow.Domain.Users;

namespace ReserveFlow.Infrastructure.Repositories;

public sealed class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        dbContext.Users.AnyAsync(u => u.Email.Value == email.Value, cancellationToken);

    public Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default) =>
        dbContext.Users.FirstOrDefaultAsync(u => u.Email.Value == email.Value, cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);
}
