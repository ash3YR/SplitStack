using SmartExpenseSplitter.Api.DTOs;

namespace SmartExpenseSplitter.Api.Services;

public interface IBalanceService
{
    Task<IReadOnlyList<UserBalanceDto>> GetGroupBalancesAsync(Guid groupId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
