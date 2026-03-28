using SmartExpenseSplitter.Api.DTOs;
using SmartExpenseSplitter.Api.Exceptions;
using SmartExpenseSplitter.Api.Models;
using SmartExpenseSplitter.Api.Repositories;

namespace SmartExpenseSplitter.Api.Services;

public class GroupService(
    IGroupRepository groupRepository,
    IUserRepository userRepository,
    IBalanceService balanceService,
    ILogger<GroupService> logger) : IGroupService
{
    private readonly ILogger<GroupService> _logger = logger;

    public async Task<GroupResponseDto> CreateGroupAsync(
        CreateGroupRequestDto request,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Group name is required.");
        }

        var group = new Group
        {
            Name = request.Name.Trim(),
            CreatedBy = authenticatedUserId,
            CreatedAt = DateTime.UtcNow
        };

        var creatorMembership = new GroupMember
        {
            GroupId = group.Id,
            UserId = authenticatedUserId
        };

        await groupRepository.AddGroupAsync(group, cancellationToken);
        await groupRepository.AddMemberAsync(creatorMembership, cancellationToken);
        await groupRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Group {GroupId} created by user {UserId}", group.Id, authenticatedUserId);

        var createdGroup = await groupRepository.GetGroupByIdWithMembersAsync(group.Id, cancellationToken)
            ?? throw new NotFoundException("The newly created group could not be loaded.");

        return MapToResponse(createdGroup);
    }

    public async Task<IReadOnlyList<GroupResponseDto>> GetUserGroupsAsync(Guid authenticatedUserId, CancellationToken cancellationToken = default)
    {
        var groups = await groupRepository.GetGroupsByUserIdAsync(authenticatedUserId, cancellationToken);
        _logger.LogInformation("Loaded {GroupCount} groups for user {UserId}", groups.Count, authenticatedUserId);
        return groups.Select(MapToResponse).ToList();
    }

    public async Task<GroupResponseDto> GetGroupByIdAsync(
        Guid groupId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var group = await groupRepository.GetGroupByIdWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        if (!group.Members.Any(member => member.UserId == authenticatedUserId))
        {
            throw new ForbiddenException("You are not allowed to view this group.");
        }

        _logger.LogInformation("Loaded group {GroupId} for user {UserId}", groupId, authenticatedUserId);

        return MapToResponse(group);
    }

    public async Task<CreateGroupJoinRequestResponseDto> CreateJoinRequestAsync(
        Guid groupId,
        AddGroupMemberRequestDto request,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            throw new BadRequestException("Member email is required.");
        }

        var group = await groupRepository.GetGroupByIdWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        if (!group.Members.Any(member => member.UserId == authenticatedUserId))
        {
            throw new ForbiddenException("You are not allowed to manage this group.");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await userRepository.GetByEmailAsync(normalizedEmail, cancellationToken)
            ?? throw new NotFoundException("No user was found with that email address.");

        if (group.Members.Any(member => member.UserId == user.Id))
        {
            throw new BadRequestException("That user is already a member of this group.");
        }

        if (user.Id == authenticatedUserId)
        {
            throw new BadRequestException("You are already in this group.");
        }

        var existingPendingRequest = await groupRepository.GetPendingJoinRequestAsync(groupId, user.Id, cancellationToken);

        if (existingPendingRequest is not null)
        {
            throw new BadRequestException("A pending join request already exists for that user.");
        }

        var joinRequest = new GroupJoinRequest
        {
            GroupId = groupId,
            RequestedByUserId = authenticatedUserId,
            TargetUserId = user.Id,
            Status = GroupJoinRequestStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await groupRepository.AddJoinRequestAsync(joinRequest, cancellationToken);

        await groupRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Join request {JoinRequestId} created for user {TargetUserId} in group {GroupId} by user {UserId}",
            joinRequest.Id,
            user.Id,
            groupId,
            authenticatedUserId);

        var createdJoinRequest = await groupRepository.GetJoinRequestByIdAsync(joinRequest.Id, cancellationToken)
            ?? throw new NotFoundException("The created join request could not be loaded.");

        return new CreateGroupJoinRequestResponseDto
        {
            Message = $"A join request has been sent to {user.Email}.",
            Request = MapJoinRequestToResponse(createdJoinRequest)
        };
    }

    public async Task<IReadOnlyList<GroupJoinRequestResponseDto>> GetGroupJoinRequestsAsync(
        Guid groupId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var group = await groupRepository.GetGroupByIdWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        if (!group.Members.Any(member => member.UserId == authenticatedUserId))
        {
            throw new ForbiddenException("You are not allowed to view join requests for this group.");
        }

        var joinRequests = await groupRepository.GetGroupJoinRequestsAsync(groupId, cancellationToken);
        return joinRequests.Select(MapJoinRequestToResponse).ToList();
    }

    public async Task<IReadOnlyList<GroupJoinRequestResponseDto>> GetIncomingJoinRequestsAsync(
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var joinRequests = await groupRepository.GetIncomingJoinRequestsAsync(authenticatedUserId, cancellationToken);
        return joinRequests.Select(MapJoinRequestToResponse).ToList();
    }

    public async Task<GroupJoinRequestResponseDto> AcceptJoinRequestAsync(
        Guid requestId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var joinRequest = await groupRepository.GetJoinRequestByIdAsync(requestId, cancellationToken)
            ?? throw new NotFoundException("Join request not found.");

        if (joinRequest.TargetUserId != authenticatedUserId)
        {
            throw new ForbiddenException("You are not allowed to respond to this join request.");
        }

        if (joinRequest.Status != GroupJoinRequestStatus.Pending)
        {
            throw new BadRequestException("This join request has already been handled.");
        }

        var group = await groupRepository.GetGroupByIdWithMembersAsync(joinRequest.GroupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        if (group.Members.Any(member => member.UserId == authenticatedUserId))
        {
            throw new BadRequestException("You are already a member of this group.");
        }

        joinRequest.Status = GroupJoinRequestStatus.Accepted;
        joinRequest.RespondedAt = DateTime.UtcNow;

        await groupRepository.AddMemberAsync(new GroupMember
        {
            GroupId = joinRequest.GroupId,
            UserId = authenticatedUserId
        }, cancellationToken);

        await groupRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Join request {JoinRequestId} accepted by user {UserId}",
            joinRequest.Id,
            authenticatedUserId);

        return MapJoinRequestToResponse(joinRequest);
    }

    public async Task<GroupJoinRequestResponseDto> RejectJoinRequestAsync(
        Guid requestId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var joinRequest = await groupRepository.GetJoinRequestByIdAsync(requestId, cancellationToken)
            ?? throw new NotFoundException("Join request not found.");

        if (joinRequest.TargetUserId != authenticatedUserId)
        {
            throw new ForbiddenException("You are not allowed to respond to this join request.");
        }

        if (joinRequest.Status != GroupJoinRequestStatus.Pending)
        {
            throw new BadRequestException("This join request has already been handled.");
        }

        joinRequest.Status = GroupJoinRequestStatus.Rejected;
        joinRequest.RespondedAt = DateTime.UtcNow;
        await groupRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "Join request {JoinRequestId} rejected by user {UserId}",
            joinRequest.Id,
            authenticatedUserId);

        return MapJoinRequestToResponse(joinRequest);
    }

    public async Task<GroupResponseDto> RemoveMemberAsync(
        Guid groupId,
        Guid memberUserId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var group = await groupRepository.GetGroupByIdWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("Group not found.");

        if (!group.Members.Any(member => member.UserId == authenticatedUserId))
        {
            throw new ForbiddenException("You are not allowed to manage this group.");
        }

        if (group.Members.All(member => member.UserId != memberUserId))
        {
            throw new NotFoundException("That member was not found in this group.");
        }

        if (group.Members.Count <= 1)
        {
            throw new BadRequestException("You cannot remove the last member from a group.");
        }

        if (group.CreatedBy == memberUserId)
        {
            throw new BadRequestException("The group creator cannot be removed until ownership transfer exists.");
        }

        var balances = await balanceService.GetGroupBalancesAsync(groupId, authenticatedUserId, cancellationToken);
        var memberBalance = balances.FirstOrDefault(balance => balance.UserId == memberUserId)
            ?? throw new NotFoundException("That member was not found in this group.");

        if (memberBalance.NetBalance != 0)
        {
            throw new BadRequestException("This member can only be removed after their balance is fully settled.");
        }

        var member = await groupRepository.GetMemberAsync(groupId, memberUserId, cancellationToken)
            ?? throw new NotFoundException("That member was not found in this group.");

        await groupRepository.RemoveMemberAsync(member, cancellationToken);
        await groupRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation(
            "User {TargetUserId} removed from group {GroupId} by user {UserId}",
            memberUserId,
            groupId,
            authenticatedUserId);

        var updatedGroup = await groupRepository.GetGroupByIdWithMembersAsync(groupId, cancellationToken)
            ?? throw new NotFoundException("The updated group could not be loaded.");

        return MapToResponse(updatedGroup);
    }

    private static GroupResponseDto MapToResponse(Group group)
    {
        return new GroupResponseDto
        {
            Id = group.Id,
            Name = group.Name,
            CreatedBy = group.CreatedBy,
            Members = group.Members
                .OrderBy(member => member.User?.Name)
                .Select(member => new GroupMemberResponseDto
                {
                    UserId = member.UserId,
                    Name = member.User?.Name ?? string.Empty,
                    Email = member.User?.Email ?? string.Empty
                })
                .ToList()
        };
    }

    private static GroupJoinRequestResponseDto MapJoinRequestToResponse(GroupJoinRequest joinRequest)
    {
        return new GroupJoinRequestResponseDto
        {
            Id = joinRequest.Id,
            GroupId = joinRequest.GroupId,
            GroupName = joinRequest.Group?.Name ?? string.Empty,
            RequestedByUserId = joinRequest.RequestedByUserId,
            RequestedByName = joinRequest.RequestedByUser?.Name ?? string.Empty,
            RequestedByEmail = joinRequest.RequestedByUser?.Email ?? string.Empty,
            TargetUserId = joinRequest.TargetUserId,
            TargetUserName = joinRequest.TargetUser?.Name ?? string.Empty,
            TargetUserEmail = joinRequest.TargetUser?.Email ?? string.Empty,
            Status = joinRequest.Status.ToString().ToLowerInvariant(),
            CreatedAt = joinRequest.CreatedAt,
            RespondedAt = joinRequest.RespondedAt
        };
    }
}
