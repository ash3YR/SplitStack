using Microsoft.EntityFrameworkCore;
using SmartExpenseSplitter.Api.Data;
using SmartExpenseSplitter.Api.Models;

namespace SmartExpenseSplitter.Api.Repositories;

public class GroupRepository(ApplicationDbContext dbContext) : IGroupRepository
{
    public async Task AddGroupAsync(Group group, CancellationToken cancellationToken = default)
    {
        await dbContext.Groups.AddAsync(group, cancellationToken);
    }

    public async Task AddMemberAsync(GroupMember member, CancellationToken cancellationToken = default)
    {
        await dbContext.GroupMembers.AddAsync(member, cancellationToken);
    }

    public async Task AddJoinRequestAsync(GroupJoinRequest joinRequest, CancellationToken cancellationToken = default)
    {
        await dbContext.GroupJoinRequests.AddAsync(joinRequest, cancellationToken);
    }

    public Task RemoveMemberAsync(GroupMember member, CancellationToken cancellationToken = default)
    {
        dbContext.GroupMembers.Remove(member);
        return Task.CompletedTask;
    }

    public async Task<bool> GroupExistsAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Groups.AnyAsync(group => group.Id == groupId, cancellationToken);
    }

    public async Task<List<Group>> GetGroupsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Groups
            .AsNoTracking()
            .Include(group => group.Members)
                .ThenInclude(member => member.User)
            .Where(group => group.Members.Any(member => member.UserId == userId))
            .OrderByDescending(group => group.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Group?> GetGroupByIdWithMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Groups
            .AsNoTracking()
            .Include(group => group.Members)
                .ThenInclude(member => member.User)
            .FirstOrDefaultAsync(group => group.Id == groupId, cancellationToken);
    }

    public async Task<GroupMember?> GetMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroupMembers
            .FirstOrDefaultAsync(member => member.GroupId == groupId && member.UserId == userId, cancellationToken);
    }

    public async Task<GroupJoinRequest?> GetPendingJoinRequestAsync(Guid groupId, Guid targetUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroupJoinRequests
            .AsNoTracking()
            .Include(joinRequest => joinRequest.Group)
            .Include(joinRequest => joinRequest.RequestedByUser)
            .Include(joinRequest => joinRequest.TargetUser)
            .FirstOrDefaultAsync(
                joinRequest => joinRequest.GroupId == groupId
                    && joinRequest.TargetUserId == targetUserId
                    && joinRequest.Status == GroupJoinRequestStatus.Pending,
                cancellationToken);
    }

    public async Task<GroupJoinRequest?> GetJoinRequestByIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroupJoinRequests
            .Include(joinRequest => joinRequest.Group)
            .Include(joinRequest => joinRequest.RequestedByUser)
            .Include(joinRequest => joinRequest.TargetUser)
            .FirstOrDefaultAsync(joinRequest => joinRequest.Id == requestId, cancellationToken);
    }

    public async Task<List<GroupJoinRequest>> GetIncomingJoinRequestsAsync(Guid targetUserId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroupJoinRequests
            .AsNoTracking()
            .Include(joinRequest => joinRequest.Group)
            .Include(joinRequest => joinRequest.RequestedByUser)
            .Include(joinRequest => joinRequest.TargetUser)
            .Where(joinRequest => joinRequest.TargetUserId == targetUserId)
            .OrderByDescending(joinRequest => joinRequest.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<GroupJoinRequest>> GetGroupJoinRequestsAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await dbContext.GroupJoinRequests
            .AsNoTracking()
            .Include(joinRequest => joinRequest.Group)
            .Include(joinRequest => joinRequest.RequestedByUser)
            .Include(joinRequest => joinRequest.TargetUser)
            .Where(joinRequest => joinRequest.GroupId == groupId)
            .OrderByDescending(joinRequest => joinRequest.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> HasMemberActivityAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var hasPaidExpenses = await dbContext.Expenses
            .AnyAsync(expense => expense.GroupId == groupId && expense.PaidBy == userId, cancellationToken);

        if (hasPaidExpenses)
        {
            return true;
        }

        return await dbContext.ExpenseSplits
            .AnyAsync(split => split.Expense!.GroupId == groupId && split.UserId == userId, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
