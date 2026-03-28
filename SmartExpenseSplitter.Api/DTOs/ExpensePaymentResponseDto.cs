namespace SmartExpenseSplitter.Api.DTOs;

public class ExpensePaymentResponseDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
