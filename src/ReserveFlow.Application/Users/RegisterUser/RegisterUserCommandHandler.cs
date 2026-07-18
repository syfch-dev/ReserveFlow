using FluentValidation;
using ReserveFlow.Application.Abstractions.Authentication;
using ReserveFlow.Application.Exceptions;
using ReserveFlow.Application.Messaging;
using ReserveFlow.Domain.Abstractions;
using ReserveFlow.Domain.Shared;
using ReserveFlow.Domain.Users;
using ValidationException = ReserveFlow.Application.Exceptions.ValidationException;

namespace ReserveFlow.Application.Users.RegisterUser;

public class RegisterUserCommandHandler:ICommandHandler<RegisterUserCommand,Guid>
{
    private readonly IValidator<RegisterUserCommand> _validator;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly TimeProvider _timeProvider;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserCommandHandler( IValidator<RegisterUserCommand> validator,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        TimeProvider timeProvider,
        IUnitOfWork unitOfWork)
    {
        _validator = validator;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _timeProvider = timeProvider;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> HandleAsync(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var validation = await _validator.ValidateAsync(command, cancellationToken);
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

        if (await _userRepository.ExistsByEmailAsync(email, cancellationToken))
        {
            throw new ConflictException("Email is already registered.");
        }

        var passwordHash = _passwordHasher.Hash(command.Password);
        var user = User.Register(email, passwordHash, _timeProvider.GetUtcNow().UtcDateTime);

        _userRepository.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return user.Id;
        
    }
}
