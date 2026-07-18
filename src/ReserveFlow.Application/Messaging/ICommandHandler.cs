namespace ReserveFlow.Application.Messaging;

public interface ICommandHandler<TCommand>
    where TCommand : ICommand
{
}

public interface ICommandHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
