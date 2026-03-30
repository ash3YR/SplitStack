namespace backend.DTOs;

public class CreateGroupJoinRequestResponseDto
{
    public string Message { get; set; } = string.Empty;

    public GroupJoinRequestResponseDto Request { get; set; } = new();
}
