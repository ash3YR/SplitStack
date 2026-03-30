using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Exceptions;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet("search")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserLookupDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<UserLookupDto>>>> SearchUsers(
        [FromQuery] string query,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await userService.SearchUsersAsync(query, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<UserLookupDto>>.FromData(response));
    }

    private Guid GetAuthenticatedUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedException("The authenticated user context is invalid.");
        }

        return userId;
    }
}
