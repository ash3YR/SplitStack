using SmartExpenseSplitter.Api.DTOs;
using SmartExpenseSplitter.Api.Exceptions;
using SmartExpenseSplitter.Api.Repositories;

namespace SmartExpenseSplitter.Api.Services;

public class UserService(IUserRepository userRepository, ILogger<UserService> logger) : IUserService
{
    private readonly ILogger<UserService> _logger = logger;

    public async Task<IReadOnlyList<UserLookupDto>> SearchUsersAsync(
        string query,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            throw new BadRequestException("Search query must be at least 2 characters long.");
        }

        var users = await userRepository.SearchByQueryAsync(query, 10, cancellationToken);
        var results = users
            .Where(user => user.Id != authenticatedUserId)
            .Select(user => new UserLookupDto
            {
                UserId = user.Id,
                Name = user.Name,
                Email = user.Email
            })
            .ToList();

        _logger.LogInformation(
            "User {UserId} searched users with query '{Query}' and received {ResultCount} results",
            authenticatedUserId,
            query.Trim(),
            results.Count);

        return results;
    }
}
