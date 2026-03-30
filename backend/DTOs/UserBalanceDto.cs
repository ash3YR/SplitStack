namespace backend.DTOs;

public class UserBalanceDto
{
    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal TotalPaid { get; set; }

    public decimal TotalOwes { get; set; }

    public decimal NetBalance { get; set; }
}
