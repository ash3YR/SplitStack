namespace backend.Services;

public interface IAuthService
{
    Task SyncUserAsync(Guid userId, string email, string name, CancellationToken cancellationToken = default);
}
