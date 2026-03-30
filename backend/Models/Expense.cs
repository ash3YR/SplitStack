using System.ComponentModel.DataAnnotations;

namespace backend.Models;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GroupId { get; set; }

    public Guid PaidBy { get; set; }

    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Group? Group { get; set; }

    public User? PaidByUser { get; set; }

    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();

    public ICollection<ExpensePayment> Payments { get; set; } = new List<ExpensePayment>();
}
