using Microsoft.AspNetCore.Mvc;
using ReserveFlow.Api.Controllers.Users;
using ReserveFlow.Application.Messaging;
using ReserveFlow.Application.Users.LoginUser;
using ReserveFlow.Application.Users.RegisterUser;

namespace ReserveFlow.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public sealed class UsersController() : ControllerBase
{
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterUserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterUserResponse>> Register(
        [FromBody] RegisterUserRequest request,
        [FromServices] ICommandHandler<RegisterUserCommand, Guid> registerUserCommandHandler,
        CancellationToken cancellationToken)
    {
        var command = new RegisterUserCommand(request.Email, request.Password);
        var userId = await registerUserCommandHandler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(Register),
            new RegisterUserResponse(userId));
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginUserResponse>> Login(
        [FromBody] LoginUserRequest request,
        [FromServices] ICommandHandler<LoginUserCommand, string> loginUserCommandHandler,
        CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand(request.Email, request.Password);
        var token = await loginUserCommandHandler.HandleAsync(command, cancellationToken);

        return Ok(new LoginUserResponse(token));
    }
}


