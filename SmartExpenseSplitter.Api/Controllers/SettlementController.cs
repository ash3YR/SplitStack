using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartExpenseSplitter.Api.Algorithms;
using SmartExpenseSplitter.Api.DTOs;
using SmartExpenseSplitter.Api.Exceptions;
using SmartExpenseSplitter.Api.Models;

namespace SmartExpenseSplitter.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/groups/{groupId:guid}/settlements")]
public class SettlementController(ISettlementService settlementService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SettlementDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SettlementDto>>>> GetSettlements(Guid groupId, CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await settlementService.GetSettlementsAsync(groupId, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SettlementDto>>.FromData(response));
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
