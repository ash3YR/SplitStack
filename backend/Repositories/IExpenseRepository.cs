using backend.Models;

namespace backend.Repositories;

public interface IExpenseRepository
{
    Task<Group?> GetGroupWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetExistingUserIdsAsync(IEnumerable<Guid> userIds, CancellationToken cancellationToken = default);

    Task AddExpenseAsync(Expense expense, CancellationToken cancellationToken = default);

    Task<List<Expense>> GetExpensesByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<ExpenseSplit>> GetSplitsByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<ExpensePayment>> GetPaymentsByGroupIdAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<Expense>> GetGroupExpensesAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<Expense?> GetExpenseByIdAsync(Guid expenseId, CancellationToken cancellationToken = default);

    Task ReplaceExpensePaymentsAsync(Guid expenseId, IEnumerable<ExpensePayment> payments, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
