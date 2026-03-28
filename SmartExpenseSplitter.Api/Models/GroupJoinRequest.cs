namespace SmartExpenseSplitter.Api.Models;

public class GroupJoinRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GroupId { get; set; }

    public Guid RequestedByUserId { get; set; }

    public Guid TargetUserId { get; set; }

    public GroupJoinRequestStatus Status { get; set; } = GroupJoinRequestStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? RespondedAt { get; set; }

    public Group? Group { get; set; }

    public User? RequestedByUser { get; set; }

    public User? TargetUser { get; set; }
}
