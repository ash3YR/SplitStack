using backend.DTOs;

namespace backend.Services;

public interface IBalanceService
{
    Task<IReadOnlyList<UserBalanceDto>> GetGroupBalancesAsync(Guid groupId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
