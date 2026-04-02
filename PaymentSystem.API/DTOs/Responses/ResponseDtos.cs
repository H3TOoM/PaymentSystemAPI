using PaymentSystem.Domain.Enums;

namespace PaymentSystem.API.DTOs.Responses;

public sealed record RegisterResponseDto(
    Guid UserId);

public sealed record LoginResponseDto(
    string Token,
    Guid UserId,
    string Email,
    string Name);

public sealed record WalletResponseDto(
    Guid Id,
    Guid UserId,
    string Currency,
    decimal Balance
    );

public sealed record TransactionResponseDto(
   Guid Id,
    string ReferenceId,
    TransactionType Type,
    TransactionStatus Status,
    decimal Amount,
    DateTime CreatedAtUtc);

public sealed record TransferResponseDto(
    Guid TransactionId,
    string Status)
{
    private TransactionStatus status;

    public TransferResponseDto(Guid transactionId, TransactionStatus status)
        : this(transactionId, status.ToString())
    {
        this.status = status;
    }
}