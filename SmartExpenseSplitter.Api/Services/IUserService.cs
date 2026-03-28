using SmartExpenseSplitter.Api.DTOs;

namespace SmartExpenseSplitter.Api.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserLookupDto>> SearchUsersAsync(string query, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
