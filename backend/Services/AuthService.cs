using Microsoft.Extensions.Logging;
using backend.Models;
using backend.Repositories;

namespace backend.Services;

public class AuthService(
    IUserRepository userRepository,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly ILogger<AuthService> _logger = logger;

    public async Task SyncUserAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        // Ensure user exists locally
        var existingUser = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken);
        if (existingUser != null)
        {
            if (existingUser.Id != userId)
            {
                _logger.LogWarning("User with email {Email} exists but ID mismatch. (DB: {DbId}, Token: {TokenId})", normalizedEmail, existingUser.Id, userId);
            }
            return;
        }

        var newUser = new User
        {
            Id = userId,
            Name = name.Trim(),
            Email = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        await userRepository.AddAsync(newUser, cancellationToken);
        await userRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User synced from Supabase successfully with id {UserId} and email {Email}", newUser.Id, newUser.Email);
    }
}
