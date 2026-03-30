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
[Route("api/group-join-requests")]
public class GroupJoinRequestsController(IGroupService groupService) : ControllerBase
{
    [HttpGet("incoming")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GroupJoinRequestResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupJoinRequestResponseDto>>>> GetIncoming(
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.GetIncomingJoinRequestsAsync(authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GroupJoinRequestResponseDto>>.FromData(response));
    }

    [HttpPost("{requestId:guid}/accept")]
    [ProducesResponseType(typeof(ApiResponse<GroupJoinRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupJoinRequestResponseDto>>> Accept(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.AcceptJoinRequestAsync(requestId, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<GroupJoinRequestResponseDto>.FromData(response));
    }

    [HttpPost("{requestId:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<GroupJoinRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupJoinRequestResponseDto>>> Reject(
        Guid requestId,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.RejectJoinRequestAsync(requestId, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<GroupJoinRequestResponseDto>.FromData(response));
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
