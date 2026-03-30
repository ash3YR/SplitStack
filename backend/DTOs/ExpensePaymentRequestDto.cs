using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class ExpensePaymentRequestDto
{
    [Required]
    public Guid UserId { get; set; }

    [Range(typeof(decimal), "0", "999999999999.99")]
    public decimal Amount { get; set; }
}
