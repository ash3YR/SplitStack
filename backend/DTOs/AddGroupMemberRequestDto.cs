using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class AddGroupMemberRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
}
