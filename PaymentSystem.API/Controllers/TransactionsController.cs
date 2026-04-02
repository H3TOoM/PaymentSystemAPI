using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentSystem.API.Common;
using PaymentSystem.API.DTOs.Requests;
using PaymentSystem.API.DTOs.Responses;
using PaymentSystem.Application.Features.Transactions.Queries.GetTransactionHistory;
using PaymentSystem.Application.Features.Transfers.Commands.TransferMoney;
using System.Linq;

namespace PaymentSystem.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("transfer")]
    public async Task<ActionResult<ApiResponse<TransferResponseDto>>> Transfer([FromBody] TransferRequestDto request, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var senderUserId = GetUserIdFromToken();
        var command = new TransferMoneyCommand(senderUserId, request.ReceiverUserId, request.Amount, request.ReferenceId);
        var result = await _mediator.Send(command, cancellationToken);

        var response = new TransferResponseDto(result.TransactionId, result.Status);
        return Ok(ApiResponse<TransferResponseDto>.SuccessResult(response, "Transfer initiated successfully.", correlationId));
    }

    [HttpGet("{userId:guid}")]
    public async Task<ActionResult<ApiResponse<List<TransactionResponseDto>>>> GetTransactionHistory(Guid userId, CancellationToken cancellationToken)
    {
        var correlationId = GetCorrelationId();
        var query = new GetTransactionHistoryQuery(userId);
        var result = await _mediator.Send(query, cancellationToken);

        var response = result.Select(t => new TransactionResponseDto(
           t.TransactionId,
           t.ReferenceId,
           t.Type,
           t.Status,
           t.Amount,
           t.CreatedAtUtc)).ToList();
            

        return Ok(ApiResponse<List<TransactionResponseDto>>.SuccessResult(response, "Transaction history retrieved successfully.", correlationId));
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
