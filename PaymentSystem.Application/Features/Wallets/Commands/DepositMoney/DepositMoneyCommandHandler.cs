using MediatR;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Enums;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Wallets.Commands.DepositMoney;

public sealed class DepositMoneyCommandHandler : IRequestHandler<DepositMoneyCommand, DepositMoneyResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionLogRepository _transactionLogRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DepositMoneyCommandHandler(
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

    public async Task<DepositMoneyResult> Handle(DepositMoneyCommand request, CancellationToken cancellationToken)
    {
        request.UserId.EnsureNotEmpty("UserId is required.");
        request.Amount.EnsurePositive("Amount must be greater than zero.");

        var user = (await _userRepository.GetByIdAsync(request.UserId, cancellationToken))
            .EnsureFound("User does not exist.");

        user.IsActive.Ensure("User is inactive.");

        var wallet = (await _walletRepository.GetByUserIdAsync(user.Id, cancellationToken)).FirstOrDefault()
            .EnsureFound("Wallet does not exist.");

        var referenceId = string.IsNullOrWhiteSpace(request.ReferenceId)
            ? $"DEP-{Guid.NewGuid():N}"
            : request.ReferenceId.Trim();

        (await _transactionRepository.GetByReferenceIdAsync(referenceId, cancellationToken) is null)
            .Ensure("Duplicate ReferenceId. The deposit has already been processed.");

        var transaction = wallet.CreateTransaction(Guid.NewGuid(), referenceId, TransactionType.Deposit, request.Amount);

        var startedLog = transaction.AddLog(Guid.NewGuid(), "DepositStarted", $"Deposit started for user {user.Id}.");
        wallet.Deposit(request.Amount);
        var creditedLog = transaction.AddLog(Guid.NewGuid(), "WalletCredited", $"Credited {request.Amount} to wallet {wallet.Id}.");
        transaction.MarkCompleted();
        var completedLog = transaction.AddLog(Guid.NewGuid(), "DepositCompleted", "Deposit completed successfully.");
        var audit = user.AddAuditLog(Guid.NewGuid(), "Deposit", "Transaction", $"Deposited {request.Amount}. ReferenceId: {referenceId}");

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionLogRepository.AddAsync(startedLog, cancellationToken);
        await _transactionLogRepository.AddAsync(creditedLog, cancellationToken);
        await _transactionLogRepository.AddAsync(completedLog, cancellationToken);
        await _auditLogRepository.AddAsync(audit, cancellationToken);

        _walletRepository.Update(wallet);
        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new DepositMoneyResult(transaction.Id, transaction.Status);
    }
}
