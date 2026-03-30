using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class ExpenseService(IExpenseRepository expenseRepository, ILogger<ExpenseService> logger) : IExpenseService
{
    private readonly ILogger<ExpenseService> _logger = logger;

    public async Task<ExpenseResponseDto> CreateExpenseAsync(
        CreateExpenseRequestDto request,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        ValidateCreateExpenseRequest(request);

        var group = await expenseRepository.GetGroupWithMembersAsync(request.GroupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        var groupMemberIds = group.Members.Select(member => member.UserId).ToHashSet();

        EnsureUserBelongsToGroup(authenticatedUserId, groupMemberIds);

        if (!groupMemberIds.Contains(request.PaidBy))
        {
            throw new BadRequestException("The paying user must belong to the group.");
        }

        var splitRequests = request.Splits
            .GroupBy(split => split.UserId)
            .Select(grouping => grouping.ToList())
            .ToList();

        if (splitRequests.Any(grouping => grouping.Count > 1))
        {
            throw new BadRequestException("Each user can only appear once in the splits list.");
        }

        var splitUserIds = splitRequests.Select(grouping => grouping[0].UserId).ToList();

        if (splitUserIds.Count == 0)
        {
            throw new BadRequestException("At least one split entry is required.");
        }

        var existingUserIds = await expenseRepository.GetExistingUserIdsAsync(splitUserIds.Append(request.PaidBy), cancellationToken);
        var existingUserSet = existingUserIds.ToHashSet();

        if (!existingUserSet.Contains(request.PaidBy))
        {
            throw new BadRequestException("The paying user does not exist.");
        }

        var missingSplitUsers = splitUserIds.Where(userId => !existingUserSet.Contains(userId)).ToList();
        if (missingSplitUsers.Count > 0)
        {
            throw new BadRequestException("One or more split users do not exist.");
        }

        if (splitUserIds.Any(userId => !groupMemberIds.Contains(userId)))
        {
            throw new BadRequestException("All split users must belong to the group.");
        }

        var normalizedSplitType = request.SplitType.Trim().ToLowerInvariant();
        var expenseSplits = normalizedSplitType switch
        {
            "equal" => BuildEqualSplits(splitUserIds, request.Amount),
            "exact" => BuildExactSplits(splitRequests.Select(grouping => grouping[0]).ToList(), request.Amount),
            _ => throw new BadRequestException("SplitType must be either 'equal' or 'exact'.")
        };

        var expense = new Expense
        {
            GroupId = request.GroupId,
            PaidBy = request.PaidBy,
            Amount = decimal.Round(request.Amount, 2, MidpointRounding.AwayFromZero),
            Description = request.Description.Trim(),
            Notes = request.Notes?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            Splits = expenseSplits,
            Payments = BuildExpensePayments(
                request.Payments ?? [],
                expenseSplits,
                request.PaidBy,
                groupMemberIds)
        };

        await expenseRepository.AddExpenseAsync(expense, cancellationToken);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Expense {ExpenseId} created in group {GroupId} by payer {PaidBy} for amount {Amount}",
            expense.Id,
            expense.GroupId,
            expense.PaidBy,
            expense.Amount);

        return MapToResponse(expense);
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetGroupExpensesAsync(
        Guid groupId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var group = await expenseRepository.GetGroupWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        var groupMemberIds = group.Members.Select(member => member.UserId).ToHashSet();
        EnsureUserBelongsToGroup(authenticatedUserId, groupMemberIds);

        var expenses = await expenseRepository.GetGroupExpensesAsync(groupId, cancellationToken);
        _logger.LogInformation("Loaded {ExpenseCount} expenses for group {GroupId}", expenses.Count, groupId);
        return expenses.Select(MapToResponse).ToList();
    }

    public async Task<ExpenseResponseDto> UpdateExpensePaymentsAsync(
        Guid expenseId,
        UpdateExpensePaymentsRequestDto request,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetExpenseByIdAsync(expenseId, cancellationToken)
            ?? throw new NotFoundException("Expense not found.");

        var groupMemberIds = expense.Group?.Members.Select(member => member.UserId).ToHashSet() ?? [];
        EnsureUserBelongsToGroup(authenticatedUserId, groupMemberIds);

        var replacementPayments = BuildExpensePayments(
            request.Payments ?? [],
            expense.Splits.ToList(),
            expense.PaidBy,
            groupMemberIds);

        foreach (var payment in replacementPayments)
        {
            payment.ExpenseId = expenseId;
        }

        await expenseRepository.ReplaceExpensePaymentsAsync(expenseId, replacementPayments, cancellationToken);
        await expenseRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Updated {PaymentCount} payments for expense {ExpenseId}",
            replacementPayments.Count,
            expenseId);

        var updatedExpense = await expenseRepository.GetExpenseByIdAsync(expenseId, cancellationToken)
            ?? throw new NotFoundException("Updated expense could not be loaded.");

        return MapToResponse(updatedExpense);
    }

    private static void ValidateCreateExpenseRequest(CreateExpenseRequestDto request)
    {
        if (request.Amount <= 0)
        {
            throw new BadRequestException("Amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            throw new BadRequestException("Description is required.");
        }

        if (request.Splits is null || request.Splits.Count == 0)
        {
            throw new BadRequestException("At least one split entry is required.");
        }
    }

    private static List<ExpenseSplit> BuildEqualSplits(IReadOnlyList<Guid> splitUserIds, decimal totalAmount)
    {
        if (splitUserIds.Count == 0)
        {
            throw new BadRequestException("Equal split requires at least one user.");
        }

        var roundedTotal = decimal.Round(totalAmount, 2, MidpointRounding.AwayFromZero);
        var baseShare = decimal.Round(roundedTotal / splitUserIds.Count, 2, MidpointRounding.ToZero);
        var splits = splitUserIds
            .Select(userId => new ExpenseSplit
            {
                UserId = userId,
                Amount = baseShare
            })
            .ToList();

        var allocated = baseShare * splitUserIds.Count;
        var remainder = roundedTotal - allocated;
        var centsToDistribute = (int)decimal.Round(remainder * 100, 0, MidpointRounding.AwayFromZero);

        for (var index = 0; index < centsToDistribute; index++)
        {
            splits[index].Amount += 0.01m;
        }

        return splits;
    }

    private static List<ExpenseSplit> BuildExactSplits(IReadOnlyList<ExpenseSplitRequestDto> splits, decimal totalAmount)
    {
        if (splits.Count == 0)
        {
            throw new BadRequestException("Exact split requires at least one user.");
        }

        var expenseSplits = new List<ExpenseSplit>(splits.Count);

        foreach (var split in splits)
        {
            if (split.Amount is null)
            {
                throw new BadRequestException("Each exact split must include an amount.");
            }

            var roundedAmount = decimal.Round(split.Amount.Value, 2, MidpointRounding.AwayFromZero);
            if (roundedAmount <= 0)
            {
                throw new BadRequestException("Each exact split amount must be greater than zero.");
            }

            expenseSplits.Add(new ExpenseSplit
            {
                UserId = split.UserId,
                Amount = roundedAmount
            });
        }

        var roundedTotal = decimal.Round(totalAmount, 2, MidpointRounding.AwayFromZero);
        var sumOfSplits = expenseSplits.Sum(split => split.Amount);

        if (sumOfSplits != roundedTotal)
        {
            throw new BadRequestException("The sum of exact split amounts must equal the total expense amount.");
        }

        return expenseSplits;
    }

    private static List<ExpensePayment> BuildExpensePayments(
        IReadOnlyList<ExpensePaymentRequestDto> payments,
        IReadOnlyList<ExpenseSplit> splits,
        Guid paidByUserId,
        HashSet<Guid> groupMemberIds)
    {
        if (payments.Count == 0)
        {
            return [];
        }

        var splitByUserId = splits.ToDictionary(split => split.UserId, split => split.Amount);
        var groupedPayments = payments
            .GroupBy(payment => payment.UserId)
            .Select(grouping => grouping.ToList())
            .ToList();

        if (groupedPayments.Any(grouping => grouping.Count > 1))
        {
            throw new BadRequestException("Each user can only appear once in the payments list.");
        }

        var expensePayments = new List<ExpensePayment>(groupedPayments.Count);

        foreach (var payment in groupedPayments.Select(grouping => grouping[0]))
        {
            if (!groupMemberIds.Contains(payment.UserId))
            {
                throw new BadRequestException("All payment users must belong to the group.");
            }

            if (!splitByUserId.TryGetValue(payment.UserId, out var userShare))
            {
                throw new BadRequestException("Only users included in the expense split can have payment progress.");
            }

            if (payment.UserId == paidByUserId)
            {
                throw new BadRequestException("The original payer does not need a paid-back amount entry.");
            }

            var roundedAmount = decimal.Round(payment.Amount, 2, MidpointRounding.AwayFromZero);

            if (roundedAmount < 0)
            {
                throw new BadRequestException("Paid amounts cannot be negative.");
            }

            if (roundedAmount == 0)
            {
                continue;
            }

            if (roundedAmount > userShare)
            {
                throw new BadRequestException("A user's paid amount cannot exceed their assigned share.");
            }

            expensePayments.Add(new ExpensePayment
            {
                UserId = payment.UserId,
                Amount = roundedAmount,
                CreatedAt = DateTime.UtcNow
            });
        }

        return expensePayments;
    }

    private static void EnsureUserBelongsToGroup(Guid userId, HashSet<Guid> groupMemberIds)
    {
        if (!groupMemberIds.Contains(userId))
        {
            throw new ForbiddenException("You are not allowed to access this group.");
        }
    }

    private static ExpenseResponseDto MapToResponse(Expense expense)
    {
        return new ExpenseResponseDto
        {
            ExpenseId = expense.Id,
            GroupId = expense.GroupId,
            PaidBy = expense.PaidBy,
            PaidByName = expense.PaidByUser?.Name ?? string.Empty,
            Amount = expense.Amount,
            Description = expense.Description,
            Notes = expense.Notes,
            CreatedAt = expense.CreatedAt,
            Splits = expense.Splits
                .OrderBy(split => split.User?.Name ?? split.UserId.ToString())
                .Select(split => new ExpenseSplitResponseDto
                {
                    UserId = split.UserId,
                    UserName = split.User?.Name ?? string.Empty,
                    Amount = split.Amount
                })
                .ToList(),
            Payments = expense.Payments
                .OrderBy(payment => payment.User?.Name ?? payment.UserId.ToString())
                .Select(payment => new ExpensePaymentResponseDto
                {
                    UserId = payment.UserId,
                    UserName = payment.User?.Name ?? string.Empty,
                    Amount = payment.Amount
                })
                .ToList()
        };
    }
}
