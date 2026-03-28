namespace SmartExpenseSplitter.Api.Models;

public class GroupMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid GroupId { get; set; }

    public User? User { get; set; }

    public Group? Group { get; set; }
}
