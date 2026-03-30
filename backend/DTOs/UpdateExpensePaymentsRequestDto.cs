using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class UpdateExpensePaymentsRequestDto
{
    [Required]
    public List<ExpensePaymentRequestDto> Payments { get; set; } = [];
}
