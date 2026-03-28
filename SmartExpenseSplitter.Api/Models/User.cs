using System.ComponentModel.DataAnnotations;

namespace SmartExpenseSplitter.Api.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Group> CreatedGroups { get; set; } = new List<Group>();

    public ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();

    public ICollection<GroupJoinRequest> SentGroupJoinRequests { get; set; } = new List<GroupJoinRequest>();

    public ICollection<GroupJoinRequest> ReceivedGroupJoinRequests { get; set; } = new List<GroupJoinRequest>();

    public ICollection<Expense> PaidExpenses { get; set; } = new List<Expense>();

    public ICollection<ExpenseSplit> ExpenseSplits { get; set; } = new List<ExpenseSplit>();

    public ICollection<ExpensePayment> ExpensePayments { get; set; } = new List<ExpensePayment>();
}
