import { useMemo, useState } from 'react'
import { expenseService } from '../services/expenseService'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCurrency } from '../utils/formatters'
import ModalShell from './ModalShell'

function UpdateExpensePaymentsModal({ expense, onClose, onPaymentsUpdated }) {
  const initialPaidAmounts = useMemo(
    () => Object.fromEntries((expense.payments ?? []).map((payment) => [payment.userId, payment.amount])),
    [expense.payments],
  )

  const [paidAmounts, setPaidAmounts] = useState(() => Object.fromEntries(
    (expense.splits ?? []).map((split) => [split.userId, String(initialPaidAmounts[split.userId] ?? '')]),
  ))
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')

    const payments = []

    for (const split of expense.splits ?? []) {
      if (split.userId === expense.paidBy) {
        continue
      }

      const amount = paidAmounts[split.userId] === '' ? 0 : Number(paidAmounts[split.userId])

      if (!Number.isFinite(amount) || amount < 0) {
        setError(`Paid amount for ${split.userName} must be zero or more.`)
        return
      }

      if (amount - split.amount > 0.001) {
        setError(`Paid amount for ${split.userName} cannot exceed ${formatCurrency(split.amount)}.`)
        return
      }

      payments.push({
        userId: split.userId,
        amount,
      })
    }

    setSubmitting(true)

    try {
      const updatedExpense = await expenseService.updateExpensePayments(expense.expenseId, { payments })
      await onPaymentsUpdated(updatedExpense)
      onClose()
    } catch (apiError) {
      setError(getApiErrorMessage(apiError, 'Unable to update paid amounts for this expense.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <ModalShell
      title="Track paid amounts"
      subtitle="Record how much each member has already paid back for this expense."
      onClose={onClose}
    >
      <form className="space-y-4" onSubmit={handleSubmit}>
        {error ? (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        ) : null}

        <div className="space-y-3">
          {(expense.splits ?? []).map((split) => {
            const isOriginalPayer = split.userId === expense.paidBy
            const paidAmount = Number(
              (expense.payments ?? []).find((payment) => payment.userId === split.userId)?.amount ?? 0,
            )

            return (
              <div key={split.userId} className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                <div className="flex flex-wrap items-center justify-between gap-3">
                  <div>
                    <p className="font-semibold text-slate-900">{split.userName || 'Unknown member'}</p>
                    <p className="text-sm text-slate-500">
                      Share:
                      {' '}
                      {formatCurrency(split.amount)}
                      {' '}
                      {'|'}
                      {' '}
                      Paid:
                      {' '}
                      {formatCurrency(paidAmount)}
                    </p>
                  </div>

                  {isOriginalPayer ? (
                    <span className="rounded-full bg-teal-50 px-3 py-1 text-xs font-semibold uppercase tracking-[0.16em] text-teal-700">
                      Covered as spender
                    </span>
                  ) : null}
                </div>

                {!isOriginalPayer ? (
                  <label className="mt-3 block">
                    <span className="mb-2 block text-sm font-medium text-slate-700">Paid amount</span>
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={paidAmounts[split.userId] ?? ''}
                      onChange={(event) => setPaidAmounts((current) => ({
                        ...current,
                        [split.userId]: event.target.value,
                      }))}
                      className="w-full rounded-xl border border-slate-200 bg-white px-3 py-2 outline-none transition focus:border-teal-400"
                      placeholder="0.00"
                    />
                  </label>
                ) : null}
              </div>
            )
          })}
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-slate-200 px-5 py-2.5 font-medium text-slate-700 transition hover:bg-slate-100"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={submitting}
            className="rounded-full bg-slate-900 px-5 py-2.5 font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {submitting ? 'Saving...' : 'Save paid amounts'}
          </button>
        </div>
      </form>
    </ModalShell>
  )
}

export default UpdateExpensePaymentsModal
