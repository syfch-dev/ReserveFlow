using ReserveFlow.Application.Messaging;

namespace ReserveFlow.Application.Users.LoginUser;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<string>;
