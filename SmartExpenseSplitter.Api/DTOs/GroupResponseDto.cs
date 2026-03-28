namespace SmartExpenseSplitter.Api.DTOs;

public class GroupResponseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid CreatedBy { get; set; }

    public List<GroupMemberResponseDto> Members { get; set; } = [];
}
