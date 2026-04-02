using MediatR;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Enums;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Transfers.Commands.TransferMoney;

public sealed class TransferMoneyCommandHandler : IRequestHandler<TransferMoneyCommand, TransferMoneyResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionLogRepository _transactionLogRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TransferMoneyCommandHandler(
        IUserRepository userRepository,
        IWalletRepository walletRepository,
        ITransactionRepository transactionRepository,
        ITransactionLogRepository transactionLogRepository,
        IAuditLogRepository auditLogRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _walletRepository = walletRepository;
        _transactionRepository = transactionRepository;
        _transactionLogRepository = transactionLogRepository;
        _auditLogRepository = auditLogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TransferMoneyResult> Handle(TransferMoneyCommand request, CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var existingTransaction = await _transactionRepository
            .GetByReferenceIdAsync(request.ReferenceId, cancellationToken);

        (existingTransaction is null).Ensure("Duplicate ReferenceId. The transfer has already been processed.");

        var sender = (await _userRepository.GetByIdAsync(request.SenderUserId, cancellationToken))
            .EnsureFound("Sender user does not exist.");

        var receiver = (await _userRepository.GetByIdAsync(request.ReceiverUserId, cancellationToken))
            .EnsureFound("Receiver user does not exist.");

        sender.IsActive.Ensure("Sender user is inactive.");
        receiver.IsActive.Ensure("Receiver user is inactive.");

        var senderWallet = (await _walletRepository.GetByUserIdAsync(sender.Id, cancellationToken)).FirstOrDefault()
            .EnsureFound("Sender wallet does not exist.");

        var receiverWallet = (await _walletRepository.GetByUserIdAsync(receiver.Id, cancellationToken)).FirstOrDefault()
            .EnsureFound("Receiver wallet does not exist.");

        (senderWallet.Balance >= request.Amount).Ensure("Insufficient balance.");

        // Build transfer as a single transaction aggregate rooted in sender wallet.
        var transaction = senderWallet.CreateTransaction(
            Guid.NewGuid(),
            request.ReferenceId,
            TransactionType.Transfer,
            request.Amount);

        var startedLog = transaction.AddLog(
            Guid.NewGuid(),
            "TransferStarted",
            $"Transfer started from {sender.Id} to {receiver.Id} with amount {request.Amount}.");

        senderWallet.Withdraw(request.Amount);

        var withdrawLog = transaction.AddLog(
            Guid.NewGuid(),
            "SenderWalletDebited",
            $"Debited {request.Amount} from sender wallet {senderWallet.Id}.");

        receiverWallet.Deposit(request.Amount);

        var depositLog = transaction.AddLog(
            Guid.NewGuid(),
            "ReceiverWalletCredited",
            $"Credited {request.Amount} to receiver wallet {receiverWallet.Id}.");

        transaction.MarkCompleted();

        var completedLog = transaction.AddLog(
            Guid.NewGuid(),
            "TransferCompleted",
            $"Transfer completed with status {transaction.Status}.");

        var senderAudit = sender.AddAuditLog(
            Guid.NewGuid(),
            "TransferSent",
            "Transaction",
            $"Sent {request.Amount} to user {receiver.Id}. ReferenceId: {request.ReferenceId}");

        var receiverAudit = receiver.AddAuditLog(
            Guid.NewGuid(),
            "TransferReceived",
            "Transaction",
            $"Received {request.Amount} from user {sender.Id}. ReferenceId: {request.ReferenceId}");

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionLogRepository.AddAsync(startedLog, cancellationToken);
        await _transactionLogRepository.AddAsync(withdrawLog, cancellationToken);
        await _transactionLogRepository.AddAsync(depositLog, cancellationToken);
        await _transactionLogRepository.AddAsync(completedLog, cancellationToken);
        await _auditLogRepository.AddAsync(senderAudit, cancellationToken);
        await _auditLogRepository.AddAsync(receiverAudit, cancellationToken);

        _walletRepository.Update(senderWallet);
        _walletRepository.Update(receiverWallet);
        _userRepository.Update(sender);
        _userRepository.Update(receiver);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TransferMoneyResult(transaction.Id, transaction.Status);
    }

    private static void ValidateRequest(TransferMoneyCommand request)
    {
        request.SenderUserId.EnsureNotEmpty("SenderUserId is required.");
        request.ReceiverUserId.EnsureNotEmpty("ReceiverUserId is required.");
        (request.SenderUserId != request.ReceiverUserId).Ensure("Sender cannot transfer to self.");
        request.Amount.EnsurePositive("Amount must be greater than zero.");
        request.ReferenceId.EnsureRequired("ReferenceId is required.");
    }
}
