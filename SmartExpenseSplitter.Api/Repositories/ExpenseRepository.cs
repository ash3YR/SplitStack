using Microsoft.EntityFrameworkCore;
using SmartExpenseSplitter.Api.Data;
using SmartExpenseSplitter.Api.Models;

namespace SmartExpenseSplitter.Api.Repositories;

public class ExpenseRepository(ApplicationDbContext dbContext) : IExpenseRepository
{
    public async Task<Group?> GetGroupWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Groups
            .Include(group => group.Members)
            .FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken);
    }

    public async Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroupMembers
            .AsNoTracking()
            .Include(groupMember => groupMember.User)
            .Where(groupMember => groupMember.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Guid>> GetExistingUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default)
    {
        var distinctUserIds = userIds.Distinct().ToList();

        return await dbContext.Users
            .Where(user => distinctUserIds.Contains(user.Id))
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task AddExpenseAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        await dbContext.Expenses.AddAsync(expense, cancellationToken);
    }

    public async Task<List<Expense>> GetExpensesByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .Where(expense => expense.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExpenseSplit>> GetSplitsByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ExpenseSplits
            .AsNoTracking()
            .Include(split => split.User)
            .Where(split => split.Expense!.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<ExpensePayment>> GetPaymentsByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.ExpensePayments
            .AsNoTracking()
            .Include(payment => payment.User)
            .Include(payment => payment.Expense)
            .Where(payment => payment.Expense!.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Expense>> GetGroupExpensesAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var expenses = await dbContext.Expenses
            .AsNoTracking()
            .Include(expense => expense.PaidByUser)
            .Where(expense => expense.GroupId == groupId)
            .ToListAsync(cancellationToken);

        var expenseIds = expenses.Select(expense => expense.Id).ToList();

        if (expenseIds.Count == 0)
        {
            return [];
        }

        var splits = await dbContext.ExpenseSplits
            .AsNoTracking()
            .Include(split => split.User)
            .Where(split => expenseIds.Contains(split.ExpenseId))
            .OrderBy(split => split.UserId)
            .ToListAsync(cancellationToken);

        var payments = await dbContext.ExpensePayments
            .AsNoTracking()
            .Include(payment => payment.User)
            .Where(payment => expenseIds.Contains(payment.ExpenseId))
            .OrderBy(payment => payment.UserId)
            .ToListAsync(cancellationToken);

        var splitsByExpenseId = splits
            .GroupBy(split => split.ExpenseId)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

        var paymentsByExpenseId = payments
            .GroupBy(payment => payment.ExpenseId)
            .ToDictionary(grouping => grouping.Key, grouping => grouping.ToList());

        foreach (var expense in expenses)
        {
            expense.Splits = splitsByExpenseId.GetValueOrDefault(expense.Id, []);
            expense.Payments = paymentsByExpenseId.GetValueOrDefault(expense.Id, []);
        }

        return expenses
            .OrderByDescending(expense => expense.CreatedAt)
            .ToList();
    }

    public async Task<Expense?> GetExpenseByIdAsync(Guid expenseId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Expenses
            .Include(expense => expense.Group)
                .ThenInclude(group => group!.Members)
            .Include(expense => expense.PaidByUser)
            .Include(expense => expense.Splits)
                .ThenInclude(split => split.User)
            .Include(expense => expense.Payments)
                .ThenInclude(payment => payment.User)
            .FirstOrDefaultAsync(expense => expense.Id == expenseId, cancellationToken);
    }

    public async Task ReplaceExpensePaymentsAsync(
        Guid expenseId,
        IEnumerable<ExpensePayment> payments,
        CancellationToken cancellationToken = default)
    {
        var existingPayments = await dbContext.ExpensePayments
            .Where(payment => payment.ExpenseId == expenseId)
            .ToListAsync(cancellationToken);

        dbContext.ExpensePayments.RemoveRange(existingPayments);

        var paymentList = payments.ToList();
        if (paymentList.Count > 0)
        {
            await dbContext.ExpensePayments.AddRangeAsync(paymentList, cancellationToken);
        }
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
