using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateGroupRequestDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
