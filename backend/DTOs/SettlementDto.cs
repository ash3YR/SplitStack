namespace backend.DTOs;

public class SettlementDto
{
    public Guid FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public decimal Amount { get; set; }
}
