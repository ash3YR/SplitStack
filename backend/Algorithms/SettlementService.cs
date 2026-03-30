using backend.DTOs;
using backend.Services;

namespace backend.Algorithms;

public interface ISettlementService
{
    Task<IReadOnlyList<SettlementDto>> GetSettlementsAsync(Guid groupId, Guid authenticatedUserId, CancellationToken cancellationToken = default);
}

public class SettlementService(IBalanceService balanceService, ILogger<SettlementService> logger) : ISettlementService
{
    private readonly ILogger<SettlementService> _logger = logger;

    public async Task<IReadOnlyList<SettlementDto>> GetSettlementsAsync(
        Guid groupId,
        Guid authenticatedUserId,
        CancellationToken cancellationToken = default)
    {
        var balances = await balanceService.GetGroupBalancesAsync(groupId, authenticatedUserId, cancellationToken);

        var creditors = balances
            .Where(balance => balance.NetBalance > 0)
            .Select(balance => new SettlementNode(balance.UserId, decimal.Round(balance.NetBalance, 2, MidpointRounding.AwayFromZero)))
            .OrderByDescending(node => node.Amount)
            .ToList();

        var debtors = balances
            .Where(balance => balance.NetBalance < 0)
            .Select(balance => new SettlementNode(balance.UserId, decimal.Round(decimal.Abs(balance.NetBalance), 2, MidpointRounding.AwayFromZero)))
            .OrderByDescending(node => node.Amount)
            .ToList();

        if (creditors.Count == 0 || debtors.Count == 0)
        {
            _logger.LogInformation("No settlements required for group {GroupId}", groupId);
            return [];
        }

        var settlements = new List<SettlementDto>();
        var creditorIndex = 0;
        var debtorIndex = 0;

        while (creditorIndex < creditors.Count && debtorIndex < debtors.Count)
        {
            var creditor = creditors[creditorIndex];
            var debtor = debtors[debtorIndex];
            var settlementAmount = decimal.Round(
                Math.Min(creditor.Amount, debtor.Amount),
                2,
                MidpointRounding.AwayFromZero);

            if (settlementAmount > 0)
            {
                settlements.Add(new SettlementDto
                {
                    FromUserId = debtor.UserId,
                    ToUserId = creditor.UserId,
                    Amount = settlementAmount
                });
            }

            creditor.Amount = decimal.Round(creditor.Amount - settlementAmount, 2, MidpointRounding.AwayFromZero);
            debtor.Amount = decimal.Round(debtor.Amount - settlementAmount, 2, MidpointRounding.AwayFromZero);

            if (creditor.Amount == 0)
            {
                creditorIndex++;
            }

            if (debtor.Amount == 0)
            {
                debtorIndex++;
            }
        }

        _logger.LogInformation("Calculated {SettlementCount} settlements for group {GroupId}", settlements.Count, groupId);
        return settlements;
    }

    private sealed class SettlementNode(Guid userId, decimal amount)
    {
        public Guid UserId { get; } = userId;

        public decimal Amount { get; set; } = amount;
    }
}
