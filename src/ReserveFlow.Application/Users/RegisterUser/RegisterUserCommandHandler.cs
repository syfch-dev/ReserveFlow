using FluentValidation;
using ReserveFlow.Application.Abstractions.Authentication;
using ReserveFlow.Application.Exceptions;
using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Shared;
using ReserveFlow.Domain.Users;
using ValidationException = ReserveFlow.Application.Exceptions.ValidationException;

namespace ReserveFlow.Application.Users.RegisterUser;

public static class RegisterUserCommandHandler
{
    public static async Task<Guid> Handle(
        RegisterUserCommand command,
        IValidator<RegisterUserCommand> validator,
        IUserRepository users,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
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
        catch (ArgumentException ex)
        {
            throw new ValidationException(ex.Message);
        }

        if (await users.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }

        var passwordHash = passwordHasher.Hash(command.Password);
        var user = User.Register(email, passwordHash, timeProvider.GetUtcNow().UtcDateTime);

        users.Add(user);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
