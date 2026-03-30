namespace backend.Models;

public class ExpensePayment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ExpenseId { get; set; }

    public Guid UserId { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Expense? Expense { get; set; }

    public User? User { get; set; }
}
