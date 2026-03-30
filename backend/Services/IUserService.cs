using backend.DTOs;

namespace backend.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserLookupDto>> SearchUsersAsync(string query, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
