import { formatCurrency } from '../utils/formatters'

function SettlementList({ settlements, membersById }) {
  if (!settlements.length) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">
        Nothing to settle right now. Everyone is squared up.
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {settlements.map((settlement, index) => (
        <div key={`${settlement.fromUserId}-${settlement.toUserId}-${index}`} className="rounded-[1.5rem] border border-slate-200 bg-white px-4 py-4">
          <p className="text-sm text-slate-600">Suggested settlement</p>
          <p className="mt-1 text-base font-semibold text-slate-900">
            {membersById[settlement.fromUserId]?.name ?? 'Unknown'}
            {' '}
            pays
            {' '}
            {membersById[settlement.toUserId]?.name ?? 'Unknown'}
            {' '}
            <span className="text-teal-700">{formatCurrency(settlement.amount)}</span>
          </p>
        </div>
      ))}
    </div>
  )
}

export default SettlementList
