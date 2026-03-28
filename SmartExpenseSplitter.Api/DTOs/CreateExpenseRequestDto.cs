using System.ComponentModel.DataAnnotations;

namespace SmartExpenseSplitter.Api.DTOs;

public class CreateExpenseRequestDto
{
    [Required]
    public Guid GroupId { get; set; }

    [Required]
    public Guid PaidBy { get; set; }

    [Range(typeof(decimal), "0.01", "999999999999.99")]
    public decimal Amount { get; set; }

    [Required]
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Notes { get; set; } = string.Empty;

    [Required]
    [RegularExpression("^(equal|exact)$", ErrorMessage = "SplitType must be either 'equal' or 'exact'.")]
    public string SplitType { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<ExpenseSplitRequestDto> Splits { get; set; } = [];

    public List<ExpensePaymentRequestDto> Payments { get; set; } = [];
}
