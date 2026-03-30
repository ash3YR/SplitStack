using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Group
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    public Guid CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? CreatedByUser { get; set; }

    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    public ICollection<GroupJoinRequest> JoinRequests { get; set; } = new List<GroupJoinRequest>();

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}
