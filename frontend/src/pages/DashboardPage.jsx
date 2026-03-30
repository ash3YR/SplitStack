import { useEffect, useState } from 'react'
import GroupCard from '../components/GroupCard'
import JoinRequestList from '../components/JoinRequestList'
import LoadingSpinner from '../components/LoadingSpinner'
import ModalShell from '../components/ModalShell'
import SectionCard from '../components/SectionCard'
import { groupJoinRequestService } from '../services/groupJoinRequestService'
import { groupService } from '../services/groupService'
import { getApiErrorMessage } from '../utils/apiError'

function DashboardPage() {
  const [groups, setGroups] = useState([])
  const [incomingRequests, setIncomingRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [requestError, setRequestError] = useState('')
  const [showCreateModal, setShowCreateModal] = useState(false)
  const [createForm, setCreateForm] = useState({ name: '' })
  const [createError, setCreateError] = useState('')
  const [creating, setCreating] = useState(false)
  const [actingRequestId, setActingRequestId] = useState('')

  const loadDashboard = async () => {
    setLoading(true)
    setError('')
    setRequestError('')

    try {
      const [groupData, requestData] = await Promise.all([
        groupService.getGroups(),
        groupJoinRequestService.getIncomingRequests(),
      ])
      setGroups(Array.isArray(groupData) ? groupData : [])
      setIncomingRequests(Array.isArray(requestData) ? requestData : [])
    } catch (apiError) {
      setError(getApiErrorMessage(
        apiError,
        'Unable to load groups. Make sure the backend exposes group list endpoints.',
      ))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadDashboard()
  }, [])

  const handleCreateGroup = async (event) => {
    event.preventDefault()
    setCreateError('')

    if (!createForm.name.trim()) {
      setCreateError('Group name is required.')
      return
    }

    setCreating(true)

    try {
      const createdGroup = await groupService.createGroup(createForm)
      setGroups((current) => [createdGroup, ...current])
      setCreateForm({ name: '' })
      setShowCreateModal(false)
    } catch (apiError) {
      setCreateError(getApiErrorMessage(
        apiError,
        'Unable to create a group. This UI expects POST /api/groups to exist.',
      ))
    } finally {
      setCreating(false)
    }
  }

  const handleAcceptRequest = async (request) => {
    setRequestError('')
    setActingRequestId(request.id)

    try {
      await groupJoinRequestService.acceptRequest(request.id)
      await loadDashboard()
    } catch (apiError) {
      setRequestError(getApiErrorMessage(apiError, 'Unable to accept that group request.'))
    } finally {
      setActingRequestId('')
    }
  }

  const handleRejectRequest = async (request) => {
    setRequestError('')
    setActingRequestId(request.id)

    try {
      await groupJoinRequestService.rejectRequest(request.id)
      await loadDashboard()
    } catch (apiError) {
      setRequestError(getApiErrorMessage(apiError, 'Unable to decline that group request.'))
    } finally {
      setActingRequestId('')
    }
  }

  return (
    <div className="space-y-8">
      <section className="grid gap-5 rounded-[2rem] border border-white/60 bg-white/70 p-6 shadow-[0_24px_60px_-32px_rgba(15,23,42,0.28)] backdrop-blur-xl lg:grid-cols-[1.25fr_0.75fr]">
        <div>
          <p className="text-sm font-semibold uppercase tracking-[0.24em] text-teal-700">Dashboard</p>
          <h2 className="mt-3 text-4xl font-semibold leading-tight text-slate-900">
            Keep every group expense, balance, and settlement in one calm place.
          </h2>
          <p className="mt-4 max-w-2xl text-base text-slate-600">
            Open a group to add expenses, inspect who owes what, and see the minimum settlement path generated from your live balances.
          </p>
        </div>
        <div className="rounded-[1.75rem] bg-gradient-to-br from-amber-100 via-white to-teal-100 p-5">
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="rounded-2xl bg-white/80 p-4 shadow-sm">
              <p className="text-sm text-slate-500">Groups</p>
              <p className="mt-2 text-3xl font-semibold text-slate-900">{groups.length}</p>
            </div>
            <div className="rounded-2xl bg-white/80 p-4 shadow-sm">
              <p className="text-sm text-slate-500">Workspace</p>
              <p className="mt-2 text-xl font-semibold text-slate-900">Frontend ready</p>
            </div>
          </div>
        </div>
      </section>

      <SectionCard
        title="Your groups"
        subtitle="Choose a group to view members, expenses, balances, and settlement suggestions."
        action={(
          <button
            type="button"
            onClick={() => setShowCreateModal(true)}
            className="rounded-full bg-slate-900 px-4 py-2.5 text-sm font-medium text-white shadow-lg shadow-slate-900/20 transition hover:bg-slate-800"
          >
            Create Group
          </button>
        )}
      >
        {loading ? (
          <div className="rounded-2xl bg-slate-50 px-4 py-8">
            <LoadingSpinner label="Loading groups..." />
          </div>
        ) : error ? (
          <div className="rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-800">
            {error}
          </div>
        ) : groups.length === 0 ? (
          <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-8 text-center text-sm text-slate-500">
            No groups found yet. Create your first group to get started.
          </div>
        ) : (
          <div className="grid gap-5 md:grid-cols-2 xl:grid-cols-3">
            {groups.map((group) => (
              <GroupCard key={group.id} group={group} />
            ))}
          </div>
        )}
      </SectionCard>

      <SectionCard
        title="Join requests"
        subtitle="Approve or decline invitations before they add you to a group."
      >
        {requestError ? (
          <div className="mb-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-4 text-sm text-amber-800">
            {requestError}
          </div>
        ) : null}
        <JoinRequestList
          requests={incomingRequests.filter((request) => request.status === 'pending')}
          emptyMessage="No pending join requests right now."
          actionLabel="Accept"
          secondaryActionLabel="Decline"
          onAction={handleAcceptRequest}
          onSecondaryAction={handleRejectRequest}
          loadingRequestId={actingRequestId}
        />
      </SectionCard>

      {showCreateModal ? (
        <ModalShell
          title="Create a group"
          subtitle="Create a fresh group and automatically join it as the owner."
          onClose={() => setShowCreateModal(false)}
        >
          <form className="space-y-4" onSubmit={handleCreateGroup}>
            {createError ? (
              <div className="rounded-2xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm text-rose-700">
                {createError}
              </div>
            ) : null}

            <label className="block">
              <span className="mb-2 block text-sm font-medium text-slate-700">Group name</span>
              <input
                type="text"
                value={createForm.name}
                onChange={(event) => setCreateForm({ name: event.target.value })}
                className="w-full rounded-2xl border border-slate-200 bg-slate-50 px-4 py-3 outline-none transition focus:border-teal-400 focus:bg-white"
                placeholder="Goa Trip"
                required
              />
            </label>

            <div className="flex justify-end gap-3 pt-2">
              <button
                type="button"
                onClick={() => setShowCreateModal(false)}
                className="rounded-full border border-slate-200 px-5 py-2.5 font-medium text-slate-700 transition hover:bg-slate-100"
              >
                Cancel
              </button>
              <button
                type="submit"
                disabled={creating}
                className="rounded-full bg-slate-900 px-5 py-2.5 font-medium text-white transition hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
              >
                {creating ? 'Creating...' : 'Create group'}
              </button>
            </div>
          </form>
        </ModalShell>
      ) : null}
    </div>
  )
}

export default DashboardPage
