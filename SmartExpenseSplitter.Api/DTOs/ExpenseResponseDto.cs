namespace SmartExpenseSplitter.Api.DTOs;

public class ExpenseResponseDto
{
    public Guid ExpenseId { get; set; }

    public Guid GroupId { get; set; }

    public Guid PaidBy { get; set; }

    public string PaidByName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<ExpenseSplitResponseDto> Splits { get; set; } = [];

    public List<ExpensePaymentResponseDto> Payments { get; set; } = [];
}
