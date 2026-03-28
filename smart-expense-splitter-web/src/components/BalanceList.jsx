import { formatCurrency } from '../utils/formatters'

function BalanceList({ balances }) {
  if (!balances.length) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">
        No balances yet. As soon as expenses are added, balances will appear here.
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {balances.map((balance) => {
        const tone = balance.netBalance > 0
          ? 'text-emerald-700 bg-emerald-50'
          : balance.netBalance < 0
            ? 'text-rose-700 bg-rose-50'
            : 'text-slate-700 bg-slate-100'

        return (
          <div key={balance.userId} className="flex flex-wrap items-center justify-between gap-3 rounded-[1.5rem] border border-slate-200 bg-white px-4 py-3">
            <div>
              <p className="font-semibold text-slate-900">{balance.name}</p>
              <p className="text-sm text-slate-500">
                Paid {formatCurrency(balance.totalPaid)} · Owes {formatCurrency(balance.totalOwes)}
              </p>
            </div>
            <span className={`rounded-full px-3 py-1 text-sm font-semibold ${tone}`}>
              {formatCurrency(balance.netBalance)}
            </span>
          </div>
        )
      })}
    </div>
  )
}

export default BalanceList
