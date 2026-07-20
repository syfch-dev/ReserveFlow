using FluentValidation;
using ReserveFlow.Application.Abstractions.Authentication;
using ReserveFlow.Application.Exceptions;
using ReserveFlow.Application.Messaging;
using ReserveFlow.Domain.Shared;
using ReserveFlow.Domain.Users;
using ValidationException = ReserveFlow.Application.Exceptions.ValidationException;

namespace ReserveFlow.Application.Users.LoginUser;

public sealed class LoginUserCommandHandler(
    IValidator<LoginUserCommand> validator,
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenProvider jwtTokenProvider) : ICommandHandler<LoginUserCommand, string>
{
    public async Task<string> HandleAsync(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
        {
            throw new ValidationException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
        }

        Email email;
        try
        {
            email = Email.Create(command.Email);
        }
        catch (ArgumentException)
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        var user = await userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !passwordHasher.Verify(command.Password, user.PasswordHash))
        {
            throw new UnauthorizedException("Invalid credentials.");
        }

        if (user.Status == UserStatus.Suspended)
        {
            throw new UnauthorizedException("Account is suspended.");
        }

        return jwtTokenProvider.Generate(user.Id, user.Email.Value, user.Roles);
    }
}
