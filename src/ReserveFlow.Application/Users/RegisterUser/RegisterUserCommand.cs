using ReserveFlow.Application.Messaging;

namespace ReserveFlow.Application.Users.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password) : ICommand<Guid>;
