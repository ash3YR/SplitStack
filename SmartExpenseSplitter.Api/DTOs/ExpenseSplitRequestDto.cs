using System.ComponentModel.DataAnnotations;

namespace SmartExpenseSplitter.Api.DTOs;

public class ExpenseSplitRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    public decimal? Amount { get; set; }
}
