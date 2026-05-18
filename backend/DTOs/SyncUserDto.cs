using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class SyncUserDto
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
