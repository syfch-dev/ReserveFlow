namespace ReserveFlow.Application.Exceptions;

public sealed class UnauthorizedException(string message) : AppException(message);
