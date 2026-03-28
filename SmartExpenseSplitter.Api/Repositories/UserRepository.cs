using Microsoft.EntityFrameworkCore;
using SmartExpenseSplitter.Api.Data;
using SmartExpenseSplitter.Api.Models;

namespace SmartExpenseSplitter.Api.Repositories;

public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task<List<User>> SearchByQueryAsync(string query, int take = 10, CancellationToken cancellationToken = default)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();

        return await dbContext.Users
            .AsNoTracking()
            .Where(user => user.Name.ToLower().Contains(normalizedQuery) || user.Email.Contains(normalizedQuery))
            .OrderBy(user => user.Name)
            .ThenBy(user => user.Email)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await dbContext.Users.AnyAsync(user => user.Email == normalizedEmail, cancellationToken);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
