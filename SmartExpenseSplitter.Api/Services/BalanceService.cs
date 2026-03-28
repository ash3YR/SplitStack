using SmartExpenseSplitter.Api.DTOs;
using SmartExpenseSplitter.Api.Exceptions;
using SmartExpenseSplitter.Api.Repositories;

namespace SmartExpenseSplitter.Api.Services;

public class BalanceService(IExpenseRepository expenseRepository, ILogger<BalanceService> logger) : IBalanceService
{
    private readonly ILogger<BalanceService> _logger = logger;

    public async Task<IReadOnlyList<UserBalanceDto>> GetGroupBalancesAsync(
        Guid groupId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var group = await expenseRepository.GetGroupWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        var groupMembers = await expenseRepository.GetGroupMembersAsync(groupId, cancellationToken);
        var groupMemberIds = groupMembers.Select(member => member.UserId).ToHashSet();

        if (!groupMemberIds.Contains(authenticatedUserId))
        {
            throw new ForbiddenException("You are not allowed to access balances for this group.");
        }

        var expenses = await expenseRepository.GetExpensesByGroupIdAsync(groupId, cancellationToken);
        var splits = await expenseRepository.GetSplitsByGroupIdAsync(groupId, cancellationToken);
        var payments = await expenseRepository.GetPaymentsByGroupIdAsync(groupId, cancellationToken);
        _logger.LogInformation(
            "Calculating balances for group {GroupId} with {MemberCount} members, {ExpenseCount} expenses, {SplitCount} splits, and {PaymentCount} payments",
            groupId,
            groupMembers.Count,
            expenses.Count,
            splits.Count,
            payments.Count);

        var totalSpentByUser = expenses
            .GroupBy(expense => expense.PaidBy)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => decimal.Round(grouping.Sum(expense => expense.Amount), 2, MidpointRounding.AwayFromZero));

        var totalOwedByUser = splits
            .GroupBy(split => split.UserId)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => decimal.Round(grouping.Sum(split => split.Amount), 2, MidpointRounding.AwayFromZero));

        var settlementPaidByUser = payments
            .GroupBy(payment => payment.UserId)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => decimal.Round(grouping.Sum(payment => payment.Amount), 2, MidpointRounding.AwayFromZero));

        var settlementReceivedByUser = payments
            .Where(payment => payment.Expense is not null)
            .GroupBy(payment => payment.Expense!.PaidBy)
            .ToDictionary(
                grouping => grouping.Key,
                grouping => decimal.Round(grouping.Sum(payment => payment.Amount), 2, MidpointRounding.AwayFromZero));

        return groupMembers
            .OrderBy(member => member.User!.Name)
            .Select(member =>
            {
                var totalPaid = totalSpentByUser.GetValueOrDefault(member.UserId, 0m);
                var totalOwes = totalOwedByUser.GetValueOrDefault(member.UserId, 0m);
                var totalSettled = settlementPaidByUser.GetValueOrDefault(member.UserId, 0m);
                var totalReceived = settlementReceivedByUser.GetValueOrDefault(member.UserId, 0m);

                return new UserBalanceDto
                {
                    UserId = member.UserId,
                    Name = member.User?.Name ?? string.Empty,
                    TotalPaid = totalPaid,
                    TotalOwes = totalOwes,
                    NetBalance = decimal.Round(totalPaid - totalOwes + totalSettled - totalReceived, 2, MidpointRounding.AwayFromZero)
                };
            })
            .ToList();
    }
}
