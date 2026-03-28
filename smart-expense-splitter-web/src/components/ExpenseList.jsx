import { formatCurrency, formatDate } from '../utils/formatters'

function ExpenseList({ expenses, membersById, onManagePayments }) {
  if (!expenses.length) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">
        No expenses yet. Add the first one to start tracking the group.
      </div>
    )
  }

  return (
    <div className="space-y-4">
      {expenses.map((expense) => (
        <article key={expense.expenseId} className="rounded-[1.5rem] border border-slate-200 bg-slate-50/70 p-4">
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <h3 className="text-lg font-semibold text-slate-900">{expense.description}</h3>
              <p className="mt-1 text-sm text-slate-600">
                Paid to vendor by
                {' '}
                <span className="font-medium text-slate-800">
                  {expense.paidByName || membersById[expense.paidBy]?.name || 'Unknown member'}
                </span>
              </p>
              {expense.notes ? (
                <p className="mt-2 text-sm text-slate-500">{expense.notes}</p>
              ) : null}
            </div>
            <div className="text-right">
              <p className="text-lg font-semibold text-slate-900">{formatCurrency(expense.amount)}</p>
              <p className="text-xs uppercase tracking-[0.18em] text-slate-500">{formatDate(expense.createdAt)}</p>
              <button
                type="button"
                onClick={() => onManagePayments(expense)}
                className="mt-3 rounded-full border border-slate-200 bg-white px-3 py-1.5 text-xs font-semibold uppercase tracking-[0.16em] text-slate-700 transition hover:bg-slate-100"
              >
                Update Paid Amounts
              </button>
            </div>
          </div>

          <div className="mt-4 grid gap-2 sm:grid-cols-2">
            {expense.splits?.map((split) => {
              const paidAmount = Number(
                expense.payments?.find((payment) => payment.userId === split.userId)?.amount ?? 0,
              )
              const remainingAmount = Math.max(0, Number(split.amount) - paidAmount)

              return (
                <div key={`${expense.expenseId}-${split.userId}`} className="rounded-2xl bg-white px-3 py-3 text-sm text-slate-700">
                  <div className="flex items-center justify-between gap-3">
                    <span className="font-medium text-slate-900">
                      {split.userName || membersById[split.userId]?.name || 'Unknown member'}
                    </span>
                    <span>{formatCurrency(split.amount)}</span>
                  </div>
                  <div className="mt-2 flex flex-wrap gap-3 text-xs font-medium uppercase tracking-[0.16em]">
                    <span className="text-emerald-700">Paid {formatCurrency(paidAmount)}</span>
                    <span className="text-amber-700">Left {formatCurrency(remainingAmount)}</span>
                  </div>
                </div>
              )
            })}
          </div>
        </article>
      ))}
    </div>
  )
}

export default ExpenseList
