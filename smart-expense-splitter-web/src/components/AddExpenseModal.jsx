import { useEffect, useState } from 'react'
import { expenseService } from '../services/expenseService'
import { getApiErrorMessage } from '../utils/apiError'
import { formatCurrency } from '../utils/formatters'
import ModalShell from './ModalShell'

function AddExpenseModal({ group, onClose, onExpenseCreated }) {
  const [form, setForm] = useState({
    amount: '',
    description: '',
    notes: '',
    paidBy: group.members?.[0]?.userId ?? '',
    splitType: 'equal',
    selectedUserIds: group.members?.map((member) => member.userId) ?? [],
    exactAmounts: {},
  })
  const [submitting, setSubmitting] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    setForm((current) => ({
      ...current,
      paidBy: current.paidBy || group.members?.[0]?.userId || '',
      selectedUserIds: current.selectedUserIds.length
        ? current.selectedUserIds
        : (group.members?.map((member) => member.userId) ?? []),
    }))
  }, [group.members])

  const handleToggleUser = (userId) => {
    setForm((current) => {
      const exists = current.selectedUserIds.includes(userId)
      const selectedUserIds = exists
        ? current.selectedUserIds.filter((id) => id !== userId)
        : [...current.selectedUserIds, userId]

      return { ...current, selectedUserIds }
    })
  }

  const handleExactAmountChange = (userId, value) => {
    setForm((current) => ({
      ...current,
      exactAmounts: {
        ...current.exactAmounts,
        [userId]: value,
      },
    }))
  }

  const buildPayload = () => {
    const selectedMembers = group.members.filter((member) => form.selectedUserIds.includes(member.userId))

    const splits = form.splitType === 'equal'
      ? selectedMembers.map((member) => ({ userId: member.userId }))
      : selectedMembers.map((member) => ({
        userId: member.userId,
        amount: Number(form.exactAmounts[member.userId] ?? 0),
      }))

    return {
      groupId: group.id,
      paidBy: form.paidBy,
      amount: Number(form.amount),
      description: form.description.trim(),
      notes: form.notes.trim(),
      splitType: form.splitType,
      splits,
    }
  }

  const handleSubmit = async (event) => {
    event.preventDefault()
    setError('')

    const amount = Number(form.amount)

    if (!Number.isFinite(amount) || amount <= 0) {
      setError('Amount must be greater than zero.')
      return
    }

    if (!form.description.trim()) {
      setError('Description is required.')
      return
    }

    if (form.selectedUserIds.length === 0) {
      setError('Select at least one member to split the expense.')
      return
    }

    if (form.splitType === 'exact') {
      const exactTotal = form.selectedUserIds.reduce(
        (sum, userId) => sum + Number(form.exactAmounts[userId] ?? 0),
        0,
      )

      if (exactTotal <= 0) {
        setError('Exact splits must add up to more than zero.')
        return
      }

      if (Math.abs(exactTotal - amount) > 0.001) {
        setError(`Exact splits must total ${formatCurrency(amount)}.`)
        return
      }
    }

    setSubmitting(true)

    try {
      await expenseService.createExpense(buildPayload())
      await onExpenseCreated()
      onClose()
    } catch (apiError) {
      setError(getApiErrorMessage(apiError, 'Unable to create the expense.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <ModalShell title="Add an expense" subtitle="Capture who paid and how the cost should be split." onClose={onClose}>
      <form className="space-y-5" onSubmit={handleSubmit}>
        {error ? (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        ) : null}

        <div className="grid gap-4 md:grid-cols-2">
          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700">Amount</span>
            <input
              type="number"
              min="0"
              step="0.01"
              value={form.amount}
              onChange={(event) => setForm((current) => ({ ...current, amount: event.target.value }))}
            className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-teal-400 focus:bg-white"
            placeholder="1500"
            required
          />
          </label>

          <label className="block">
            <span className="mb-2 block text-sm font-medium text-slate-700">Paid by</span>
            <select
              value={form.paidBy}
              onChange={(event) => setForm((current) => ({ ...current, paidBy: event.target.value }))}
              className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-teal-400 focus:bg-white"
              required
            >
              {group.members.map((member) => (
                <option key={member.userId} value={member.userId}>
                  {member.name}
                </option>
              ))}
            </select>
          </label>
        </div>

        <label className="block">
          <span className="mb-2 block text-sm font-medium text-slate-700">Description</span>
          <input
            type="text"
            value={form.description}
            onChange={(event) => setForm((current) => ({ ...current, description: event.target.value }))}
            className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-teal-400 focus:bg-white"
            placeholder="Dinner at Lakeview Cafe"
            required
          />
        </label>

        <label className="block">
          <span className="mb-2 block text-sm font-medium text-slate-700">Notes</span>
          <textarea
            value={form.notes}
            onChange={(event) => setForm((current) => ({ ...current, notes: event.target.value }))}
            className="min-h-24 w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-teal-400 focus:bg-white"
            placeholder="More notes for lunch, shared dishes, receipts, or anything helpful..."
            maxLength={1000}
          />
        </label>

        <div>
          <span className="mb-2 block text-sm font-medium text-slate-700">Split type</span>
          <div className="grid gap-3 sm:grid-cols-2">
            {['equal', 'exact'].map((option) => (
              <button
                key={option}
                type="button"
                onClick={() => setForm((current) => ({ ...current, splitType: option }))}
                className={`rounded-2xl border px-4 py-3 text-left transition ${
                  form.splitType === option
                    ? 'border-teal-500 bg-teal-50 text-teal-900'
                    : 'border-slate-200 bg-slate-50 text-slate-700 hover:bg-white'
                }`}
              >
                <span className="block font-semibold capitalize">{option}</span>
                <span className="mt-1 block text-sm">
                  {option === 'equal' ? 'Backend divides the amount evenly.' : 'Enter an exact amount for each member.'}
                </span>
              </button>
            ))}
          </div>
        </div>

        <div>
          <span className="mb-3 block text-sm font-medium text-slate-700">Split members</span>
          <div className="grid gap-3 md:grid-cols-2">
            {group.members.map((member) => {
              const selected = form.selectedUserIds.includes(member.userId)

              return (
                <div key={member.userId} className="rounded-2xl border border-slate-200 bg-slate-50 p-4">
                  <label className="flex items-center gap-3">
                    <input
                      type="checkbox"
                      checked={selected}
                      onChange={() => handleToggleUser(member.userId)}
                      className="h-4 w-4 rounded border-slate-300 text-teal-600 focus:ring-teal-500"
                    />
                    <span className="font-medium text-slate-900">{member.name}</span>
                  </label>

                  {form.splitType === 'exact' && selected ? (
                    <input
                      type="number"
                      min="0"
                      step="0.01"
                      value={form.exactAmounts[member.userId] ?? ''}
                      onChange={(event) => handleExactAmountChange(member.userId, event.target.value)}
                      className="mt-3 w-full rounded-xl border border-slate-200 bg-white px-3 py-2 outline-none transition focus:border-teal-400"
                      placeholder="Exact share"
                      required
                    />
                  ) : null}
                </div>
              )
            })}
          </div>
        </div>

        {form.splitType === 'exact' ? (
          <div className="rounded-2xl bg-slate-50 px-4 py-3 text-sm text-slate-600">
            Exact split total:
            {' '}
            <span className="font-semibold text-slate-900">
              {formatCurrency(form.selectedUserIds.reduce(
                (sum, userId) => sum + Number(form.exactAmounts[userId] ?? 0),
                0,
              ))}
            </span>
          </div>
        ) : null}

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
            className="rounded-full bg-slate-900 px-5 py-2.5 font-medium text-white shadow-lg shadow-slate-900/20 transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {submitting ? 'Saving...' : 'Create expense'}
          </button>
        </div>
      </form>
    </ModalShell>
  )
}

export default AddExpenseModal
