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
[Route("api/groups")]
public class GroupController(IGroupService groupService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GroupResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<GroupResponseDto>>> CreateGroup(
        CreateGroupRequestDto request,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.CreateGroupAsync(request, authenticatedUserId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<GroupResponseDto>.FromData(response));
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GroupResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupResponseDto>>>> GetUserGroups(CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.GetUserGroupsAsync(authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GroupResponseDto>>.FromData(response));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GroupResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupResponseDto>>> GetGroupById(Guid id, CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.GetGroupByIdAsync(id, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<GroupResponseDto>.FromData(response));
    }

    [HttpPost("{id:guid}/members")]
    [HttpPost("{id:guid}/join-requests")]
    [ProducesResponseType(typeof(ApiResponse<CreateGroupJoinRequestResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<CreateGroupJoinRequestResponseDto>>> AddMember(
        Guid id,
        AddGroupMemberRequestDto request,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.CreateJoinRequestAsync(id, request, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<CreateGroupJoinRequestResponseDto>.FromData(response));
    }

    [HttpGet("{id:guid}/join-requests")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GroupJoinRequestResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<GroupJoinRequestResponseDto>>>> GetGroupJoinRequests(
        Guid id,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.GetGroupJoinRequestsAsync(id, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<GroupJoinRequestResponseDto>>.FromData(response));
    }

    [HttpDelete("{id:guid}/members/{memberUserId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GroupResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<GroupResponseDto>>> RemoveMember(
        Guid id,
        Guid memberUserId,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await groupService.RemoveMemberAsync(id, memberUserId, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<GroupResponseDto>.FromData(response));
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
