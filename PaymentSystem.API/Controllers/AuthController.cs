using MediatR;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.API.Common;
using PaymentSystem.API.DTOs.Requests;
using PaymentSystem.API.DTOs.Responses;
using PaymentSystem.Application.Features.Authentication.Commands.LoginUser;
using PaymentSystem.Application.Features.Authentication.Commands.RegisterUser;

namespace PaymentSystem.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        var response = new RegisterResponseDto(result.UserId);
        return Ok(ApiResponse<RegisterResponseDto>.SuccessResult(response, "User registered successfully.", correlationId));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var command = new LoginUserCommand(request.Email, request.Password);
        var result = await _mediator.Send(command, cancellationToken);

        var response = new LoginResponseDto(result.Token, result.UserId, result.Email, result.Name);
        return Ok(ApiResponse<LoginResponseDto>.SuccessResult(response, "Login successful.", correlationId));
    }

    private string GetCorrelationId()
    {
        return Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId)
            ? correlationId!
            : Guid.NewGuid().ToString();
    }
}
