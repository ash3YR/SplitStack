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
[Route("api")]
public class ExpenseController(IExpenseService expenseService) : ControllerBase
{
    [HttpPost("expenses")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseResponseDto>>> CreateExpense(
        CreateExpenseRequestDto request,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await expenseService.CreateExpenseAsync(request, authenticatedUserId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<ExpenseResponseDto>.FromData(response));
    }

    [HttpGet("groups/{groupId:guid}/expenses")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ExpenseResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<ExpenseResponseDto>>>> GetGroupExpenses(
        Guid groupId,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await expenseService.GetGroupExpensesAsync(groupId, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ExpenseResponseDto>>.FromData(response));
    }

    [HttpPut("expenses/{expenseId:guid}/payments")]
    [ProducesResponseType(typeof(ApiResponse<ExpenseResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ExpenseResponseDto>>> UpdateExpensePayments(
        Guid expenseId,
        UpdateExpensePaymentsRequestDto request,
        CancellationToken cancellationToken)
    {
        var authenticatedUserId = GetAuthenticatedUserId();
        var response = await expenseService.UpdateExpensePaymentsAsync(expenseId, request, authenticatedUserId, cancellationToken);
        return Ok(ApiResponse<ExpenseResponseDto>.FromData(response));
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
