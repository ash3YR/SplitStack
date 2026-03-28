using SmartExpenseSplitter.Api.DTOs;

namespace SmartExpenseSplitter.Api.Services;

public interface IGroupService
{
    Task<GroupResponseDto> CreateGroupAsync(CreateGroupRequestDto request, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroupResponseDto>> GetUserGroupsAsync(Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<GroupResponseDto> GetGroupByIdAsync(Guid groupId, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<CreateGroupJoinRequestResponseDto> CreateJoinRequestAsync(Guid groupId, AddGroupMemberRequestDto request, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroupJoinRequestResponseDto>> GetGroupJoinRequestsAsync(Guid groupId, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<GroupJoinRequestResponseDto>> GetIncomingJoinRequestsAsync(Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<GroupJoinRequestResponseDto> AcceptJoinRequestAsync(Guid requestId, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<GroupJoinRequestResponseDto> RejectJoinRequestAsync(Guid requestId, Guid authenticatedUserId, CancellationToken cancellationToken = default);

    Task<GroupResponseDto> RemoveMemberAsync(Guid groupId, Guid memberUserId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}
