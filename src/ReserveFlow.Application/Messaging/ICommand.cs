

namespace ReserveFlow.Application.Messaging;

public interface ICommand : IBaseCommand
{
}

public interface ICommand<TReponse> : IBaseCommand
{
}

public interface IBaseCommand
{
}
