using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("sync")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Sync([FromBody] SyncUserDto request, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var emailClaim = User.FindFirstValue(ClaimTypes.Email);

        if (string.IsNullOrEmpty(userIdClaim) || string.IsNullOrEmpty(emailClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return BadRequest("Invalid JWT claims. Subject/Email are required.");
        }

        await authService.SyncUserAsync(userId, emailClaim, request.Name, cancellationToken);
        return Ok(new { message = "User synced successfully." });
    }
}
