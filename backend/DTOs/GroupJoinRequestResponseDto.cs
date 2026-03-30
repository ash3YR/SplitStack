namespace backend.DTOs;

public class GroupJoinRequestResponseDto
{
    public Guid Id { get; set; }

    public Guid GroupId { get; set; }

    public string GroupName { get; set; } = string.Empty;

    public Guid RequestedByUserId { get; set; }

    public string RequestedByName { get; set; } = string.Empty;

    public string RequestedByEmail { get; set; } = string.Empty;

    public Guid TargetUserId { get; set; }

    public string TargetUserName { get; set; } = string.Empty;

    public string TargetUserEmail { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? RespondedAt { get; set; }
}
