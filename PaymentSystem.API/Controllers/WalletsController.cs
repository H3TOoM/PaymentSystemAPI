using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.API.Common;
using PaymentSystem.API.DTOs.Requests;
using PaymentSystem.API.DTOs.Responses;
using PaymentSystem.Application.Features.Wallets.Commands.DepositMoney;
using PaymentSystem.Application.Features.Wallets.Commands.WithdrawMoney;
using PaymentSystem.Application.Features.Wallets.Queries.GetWalletDetails;

namespace PaymentSystem.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class WalletsController : ControllerBase
{
    private readonly IMediator _mediator;

    public WalletsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<WalletResponseDto>>> GetWalletByUserId(Guid userId, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var query = new GetWalletDetailsQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        var response = new WalletResponseDto(
            result.WalletId,
            result.UserId,
            result.Currency,
            result.Balance
            );

        return Ok(ApiResponse<WalletResponseDto>.SuccessResult(response, "Wallet retrieved successfully.", correlationId));
    }

    [HttpPost("deposit")]
    public async Task<ActionResult<ApiResponse<object?>>> Deposit([FromBody] DepositRequestDto request, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var userId = GetUserIdFromToken();
        var command = new DepositMoneyCommand(userId, request.Amount);
        await _mediator.Send(command, cancellationToken);

        return Ok(ApiResponse<object?>.SuccessResult(null, "Deposit successful.", correlationId));
    }

    [HttpPost("withdraw")]
    public async Task<ActionResult<ApiResponse<object?>>> Withdraw([FromBody] WithdrawRequestDto request, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var userId = GetUserIdFromToken();
        var command = new WithdrawMoneyCommand(userId, request.Amount);
        await _mediator.Send(command, cancellationToken);

        return Ok(ApiResponse<object?>.SuccessResult(null, "Withdrawal successful.", correlationId));
    }

    private string GetCorrelationId()
    {
        return Response.Headers.TryGetValue("X-Correlation-ID", out var correlationId)
            ? correlationId!
            : Guid.NewGuid().ToString();
    }

    private Guid GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        return userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId) ? userId : Guid.Empty;
    }
}
