using backend.DTOs;

namespace backend.Services;

public interface IExpenseService
{
    Task<ExpenseResponseDto> CreateExpenseAsync(CreateExpenseRequestDto request, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseResponseDto>> GetGroupExpensesAsync(Guid groupId, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<ExpenseResponseDto> UpdateExpensePaymentsAsync(Guid expenseId, UpdateExpensePaymentsRequestDto request, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
