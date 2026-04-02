namespace PaymentSystem.API.DTOs.Requests;

public sealed record RegisterRequestDto(
    string Name,
    string Email,
    string Password);

public sealed record LoginRequestDto(
    string Email,
    string Password);

public sealed record TransferRequestDto(
    Guid ReceiverUserId,
    decimal Amount,
    string ReferenceId);

public sealed record DepositRequestDto(
    decimal Amount);

public sealed record WithdrawRequestDto(
    decimal Amount);
