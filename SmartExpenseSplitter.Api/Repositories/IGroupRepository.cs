using SmartExpenseSplitter.Api.Models;

namespace SmartExpenseSplitter.Api.Repositories;

public interface IGroupRepository
{
    Task AddGroupAsync(Group group, CancellationToken cancellationToken = default);

    Task AddMemberAsync(GroupMember member, CancellationToken cancellationToken = default);

    Task AddJoinRequestAsync(GroupJoinRequest joinRequest, CancellationToken cancellationToken = default);

    Task RemoveMemberAsync(GroupMember member, CancellationToken cancellationToken = default);

    Task<bool> GroupExistsAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<List<Group>> GetGroupsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Group?> GetGroupByIdWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);

    Task<GroupJoinRequest?> GetPendingJoinRequestAsync(Guid groupId, Guid targetUserId, CancellationToken cancellationToken = default);

    Task<GroupJoinRequest?> GetJoinRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default);

    Task<List<GroupJoinRequest>> GetIncomingJoinRequestsAsync(Guid targetUserId, CancellationToken cancellationToken = default);

    Task<List<GroupJoinRequest>> GetGroupJoinRequestsAsync(Guid groupId, CancellationToken cancellationToken = default);

    Task<bool> HasMemberActivityAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}
