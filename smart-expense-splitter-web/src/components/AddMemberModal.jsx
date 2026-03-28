import { useEffect, useMemo, useState } from 'react'
import { groupService } from '../services/groupService'
import { userService } from '../services/userService'
import { getApiErrorMessage } from '../utils/apiError'
import ModalShell from './ModalShell'

function AddMemberModal({ group, joinRequests, onClose, onRequestCreated }) {
  const [query, setQuery] = useState('')
  const [results, setResults] = useState([])
  const [searching, setSearching] = useState(false)
  const [error, setError] = useState('')
  const [successMessage, setSuccessMessage] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const currentMemberEmails = useMemo(
    () => new Set((group.members ?? []).map((member) => member.email.toLowerCase())),
    [group.members],
  )

  const pendingRequestEmails = useMemo(
    () => new Set(
      (joinRequests ?? [])
        .filter((request) => request.status === 'pending')
        .map((request) => request.targetUserEmail.toLowerCase()),
    ),
    [joinRequests],
  )

  useEffect(() => {
    let cancelled = false

    const runSearch = async () => {
      const normalizedQuery = query.trim()

      if (normalizedQuery.length < 2) {
        setResults([])
        setSearching(false)
        setSuccessMessage('')
        return
      }

      setSearching(true)
      setError('')

      try {
        const data = await userService.searchUsers(normalizedQuery)

        if (cancelled) {
          return
        }

        const filtered = Array.isArray(data)
          ? data.filter((user) => (
            !currentMemberEmails.has(user.email.toLowerCase())
            && !pendingRequestEmails.has(user.email.toLowerCase())
          ))
          : []

        setResults(filtered)
      } catch (apiError) {
        if (!cancelled) {
          setError(getApiErrorMessage(apiError, 'Unable to search for users right now.'))
        }
      } finally {
        if (!cancelled) {
          setSearching(false)
        }
      }
    }

    const timeoutId = setTimeout(runSearch, 250)

    return () => {
      cancelled = true
      clearTimeout(timeoutId)
    }
  }, [currentMemberEmails, pendingRequestEmails, query])

  const handleAddMember = async (user) => {
    setError('')
    setSuccessMessage('')
    setSubmitting(true)

    try {
      const response = await groupService.addMember(group.id, { email: user.email })
      setSuccessMessage(response.message || `Request sent to ${user.email}.`)
      setQuery('')
      setResults([])
      await onRequestCreated(response.request)
    } catch (apiError) {
      setError(getApiErrorMessage(apiError, 'Unable to add that member to the group.'))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <ModalShell
      title="Send a join request"
      subtitle="Search by name or email, then send a request the user can accept or decline."
      onClose={onClose}
    >
      <div className="space-y-4">
        {error ? (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
            {error}
          </div>
        ) : null}

        {successMessage ? (
          <div className="rounded-2xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">
            {successMessage}
          </div>
        ) : null}

        <label className="block">
          <span className="mb-2 block text-sm font-medium text-slate-700">Search users</span>
          <input
            type="text"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-teal-400 focus:bg-white"
            placeholder="Type a name or email"
          />
        </label>

        <div className="rounded-[1.5rem] border border-slate-200 bg-slate-50 p-4">
          {query.trim().length < 2 ? (
            <p className="text-sm text-slate-500">Enter at least 2 characters to search for registered users.</p>
          ) : searching ? (
            <p className="text-sm text-slate-500">Searching users...</p>
          ) : results.length === 0 ? (
            <p className="text-sm text-slate-500">No matching registered users are available to add.</p>
          ) : (
            <div className="space-y-3">
              {results.map((user) => (
                <div
                  key={user.userId}
                  className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-slate-200 bg-white px-4 py-3"
                >
                  <div>
                    <p className="font-semibold text-slate-900">{user.name}</p>
                    <p className="text-sm text-slate-500">{user.email}</p>
                  </div>
                  <button
                    type="button"
                    onClick={() => handleAddMember(user)}
                    disabled={submitting}
                    className="rounded-full bg-slate-900 px-4 py-2 text-sm font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {submitting ? 'Sending...' : 'Send Request'}
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="flex justify-end gap-3 pt-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-full border border-slate-200 px-5 py-2.5 font-medium text-slate-700 transition hover:bg-slate-100"
          >
            Cancel
          </button>
        </div>
      </div>
    </ModalShell>
  )
}

export default AddMemberModal
