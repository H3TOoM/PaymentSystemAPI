using MediatR;
using PaymentSystem.Application.Common.Extensions;
using PaymentSystem.Domain.Enums;
using PaymentSystem.Domain.Interfaces;

namespace PaymentSystem.Application.Features.Wallets.Commands.WithdrawMoney;

public sealed class WithdrawMoneyCommandHandler : IRequestHandler<WithdrawMoneyCommand, WithdrawMoneyResult>
{
    private readonly IUserRepository _userRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly ITransactionLogRepository _transactionLogRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public WithdrawMoneyCommandHandler(
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

    public async Task<WithdrawMoneyResult> Handle(WithdrawMoneyCommand request, CancellationToken cancellationToken)
    {
        request.UserId.EnsureNotEmpty("UserId is required.");
        request.Amount.EnsurePositive("Amount must be greater than zero.");

        var user = (await _userRepository.GetByIdAsync(request.UserId, cancellationToken))
            .EnsureFound("User does not exist.");

        user.IsActive.Ensure("User is inactive.");

        var wallet = (await _walletRepository.GetByUserIdAsync(user.Id, cancellationToken)).FirstOrDefault()
            .EnsureFound("Wallet does not exist.");

        (wallet.Balance >= request.Amount).Ensure("Insufficient balance.");

        var referenceId = string.IsNullOrWhiteSpace(request.ReferenceId)
            ? $"WDR-{Guid.NewGuid():N}"
            : request.ReferenceId.Trim();

        (await _transactionRepository.GetByReferenceIdAsync(referenceId, cancellationToken) is null)
            .Ensure("Duplicate ReferenceId. The withdrawal has already been processed.");

        var transaction = wallet.CreateTransaction(Guid.NewGuid(), referenceId, TransactionType.Withdrawal, request.Amount);

        var startedLog = transaction.AddLog(Guid.NewGuid(), "WithdrawStarted", $"Withdrawal started for user {user.Id}.");
        wallet.Withdraw(request.Amount);
        var debitedLog = transaction.AddLog(Guid.NewGuid(), "WalletDebited", $"Debited {request.Amount} from wallet {wallet.Id}.");
        transaction.MarkCompleted();
        var completedLog = transaction.AddLog(Guid.NewGuid(), "WithdrawCompleted", "Withdrawal completed successfully.");
        var audit = user.AddAuditLog(Guid.NewGuid(), "Withdraw", "Transaction", $"Withdrew {request.Amount}. ReferenceId: {referenceId}");

        await _transactionRepository.AddAsync(transaction, cancellationToken);
        await _transactionLogRepository.AddAsync(startedLog, cancellationToken);
        await _transactionLogRepository.AddAsync(debitedLog, cancellationToken);
        await _transactionLogRepository.AddAsync(completedLog, cancellationToken);
        await _auditLogRepository.AddAsync(audit, cancellationToken);

        _walletRepository.Update(wallet);
        _userRepository.Update(user);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return new WithdrawMoneyResult(transaction.Id, transaction.Status);
    }
}
