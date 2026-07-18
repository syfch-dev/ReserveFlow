using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Shared;

namespace ReserveFlow.Domain.Users;

public sealed record UserRegisteredDomainEvent(
    Guid UserId,
    string Email,
    DateTime OccurredOnUtc) : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
}
