using System.ComponentModel.DataAnnotations;

namespace SmartExpenseSplitter.Api.DTOs;

public class UpdateExpensePaymentsRequestDto
{
    [Required]
    public List<ExpensePaymentRequestDto> Payments { get; set; } = [];
}
