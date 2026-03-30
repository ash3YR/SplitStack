namespace backend.DTOs;

public class ExpenseSplitResponseDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public decimal Amount { get; set; }
}
