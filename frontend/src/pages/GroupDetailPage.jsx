import { useEffect, useMemo, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import AddMemberModal from '../components/AddMemberModal'
import AddExpenseModal from '../components/AddExpenseModal'
import BalanceList from '../components/BalanceList'
import ExpenseList from '../components/ExpenseList'
import JoinRequestList from '../components/JoinRequestList'
import LoadingSpinner from '../components/LoadingSpinner'
import SectionCard from '../components/SectionCard'
import SettlementList from '../components/SettlementList'
import UpdateExpensePaymentsModal from '../components/UpdateExpensePaymentsModal'
import { balanceService } from '../services/balanceService'
import { expenseService } from '../services/expenseService'
import { groupService } from '../services/groupService'
import { settlementService } from '../services/settlementService'
import { getApiErrorMessage } from '../utils/apiError'
import { useAuth } from '../hooks/useAuth'

function GroupDetailPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const [group, setGroup] = useState(null)
  const [expenses, setExpenses] = useState([])
  const [balances, setBalances] = useState([])
  const [settlements, setSettlements] = useState([])
  const [joinRequests, setJoinRequests] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [showAddExpense, setShowAddExpense] = useState(false)
  const [showAddMember, setShowAddMember] = useState(false)
  const [selectedExpense, setSelectedExpense] = useState(null)
  const [memberActionError, setMemberActionError] = useState('')
  const [removingMemberId, setRemovingMemberId] = useState('')

  const membersById = useMemo(
    () => Object.fromEntries((group?.members ?? []).map((member) => [member.userId, member])),
    [group],
  )

  const loadGroupWorkspace = async () => {
    setLoading(true)
    setError('')

    try {
      const [groupData, expenseData, balanceData, settlementData] = await Promise.all([
        groupService.getGroupById(id),
        expenseService.getGroupExpenses(id),
        balanceService.getGroupBalances(id),
        settlementService.getGroupSettlements(id),
      ])
      const joinRequestData = await groupService.getGroupJoinRequests(id)

      setGroup(groupData)
      setExpenses(Array.isArray(expenseData) ? expenseData : [])
      setBalances(Array.isArray(balanceData) ? balanceData : [])
      setSettlements(Array.isArray(settlementData) ? settlementData : [])
      setJoinRequests(Array.isArray(joinRequestData) ? joinRequestData : [])
    } catch (apiError) {
      setError(getApiErrorMessage(
        apiError,
        'Unable to load this group. The UI expects GET /api/groups/{id} to return group details with members.',
      ))
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    loadGroupWorkspace()
  }, [id])

  const handleExpenseCreated = async () => {
    const [expenseData, balanceData, settlementData] = await Promise.all([
      expenseService.getGroupExpenses(id),
      balanceService.getGroupBalances(id),
      settlementService.getGroupSettlements(id),
    ])

    setExpenses(Array.isArray(expenseData) ? expenseData : [])
    setBalances(Array.isArray(balanceData) ? balanceData : [])
    setSettlements(Array.isArray(settlementData) ? settlementData : [])
  }

  const handleExpensePaymentsUpdated = async (updatedExpense) => {
    setExpenses((current) => current.map((expense) => (
      expense.expenseId === updatedExpense.expenseId ? updatedExpense : expense
    )))

    const [balanceData, settlementData] = await Promise.all([
      balanceService.getGroupBalances(id),
      settlementService.getGroupSettlements(id),
    ])

    setBalances(Array.isArray(balanceData) ? balanceData : [])
    setSettlements(Array.isArray(settlementData) ? settlementData : [])
  }

  const handleRequestCreated = async (createdRequest) => {
    setMemberActionError('')
    setJoinRequests((current) => [createdRequest, ...current])
  }

  const handleRemoveMember = async (member) => {
    setMemberActionError('')
    setRemovingMemberId(member.userId)

    try {
      const updatedGroup = await groupService.removeMember(id, member.userId)
      const joinRequestData = await groupService.getGroupJoinRequests(id)
      setGroup(updatedGroup)
      setJoinRequests(Array.isArray(joinRequestData) ? joinRequestData : [])

      const [balanceData, settlementData] = await Promise.all([
        balanceService.getGroupBalances(id),
        settlementService.getGroupSettlements(id),
      ])

      setBalances(Array.isArray(balanceData) ? balanceData : [])
      setSettlements(Array.isArray(settlementData) ? settlementData : [])
    } catch (apiError) {
      setMemberActionError(getApiErrorMessage(
        apiError,
        'Unable to remove that member from the group.',
      ))
    } finally {
      setRemovingMemberId('')
    }
  }

  if (loading) {
    return (
      <div className="rounded-[2rem] border border-white/70 bg-white/80 px-6 py-12 text-center text-sm text-slate-500 shadow-[0_24px_60px_-32px_rgba(15,23,42,0.28)] backdrop-blur-xl">
        <LoadingSpinner label="Loading group workspace..." />
      </div>
    )
  }

  if (error) {
    return (
      <div className="space-y-5">
        <Link to="/dashboard" className="inline-flex rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 hover:bg-slate-100">
          {'<-'} Back to dashboard
        </Link>
        <div className="rounded-[2rem] border border-amber-200 bg-amber-50 px-6 py-6 text-sm text-amber-800">
          {error}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-start justify-between gap-4 rounded-[2rem] border border-white/60 bg-white/75 p-6 shadow-[0_24px_60px_-32px_rgba(15,23,42,0.28)] backdrop-blur-xl">
        <div>
          <Link to="/dashboard" className="text-sm font-medium text-teal-700 hover:text-teal-800">
            {'<-'} Back to dashboard
          </Link>
          <p className="mt-4 text-sm font-semibold uppercase tracking-[0.24em] text-teal-700">Group workspace</p>
          <h2 className="mt-2 text-4xl font-semibold text-slate-900">{group?.name}</h2>
          <p className="mt-3 max-w-2xl text-base text-slate-600">
            Review member participation, track expenses, and understand exactly how this group should settle up.
          </p>
        </div>
        <button
          type="button"
          onClick={() => setShowAddExpense(true)}
          className="rounded-full bg-slate-900 px-5 py-3 text-sm font-medium text-white shadow-lg shadow-slate-900/20 transition hover:bg-slate-800"
        >
          Add Expense
        </button>
      </div>

      <div className="grid gap-8 xl:grid-cols-[1.2fr_0.8fr]">
        <SectionCard
          title="Group members"
          subtitle={`${group?.members?.length ?? 0} people currently take part in this split.`}
          action={(
            <button
              type="button"
              onClick={() => setShowAddMember(true)}
              className="rounded-full border border-slate-200 bg-white px-4 py-2.5 text-sm font-medium text-slate-700 transition hover:bg-slate-100"
            >
              Add Member
            </button>
          )}
        >
          {memberActionError ? (
            <div className="mb-4 rounded-2xl border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-800">
              {memberActionError}
            </div>
          ) : null}

          <div className="grid gap-3 md:grid-cols-2">
            {group?.members?.map((member) => (
              <div key={member.userId} className="rounded-[1.5rem] border border-slate-200 bg-slate-50 px-4 py-3">
                <div className="flex flex-wrap items-start justify-between gap-3">
                  <div>
                    <p className="font-semibold text-slate-900">{member.name}</p>
                    <p className="text-sm text-slate-500">{member.email || 'Group member'}</p>
                    {group?.createdBy === member.userId ? (
                      <p className="mt-2 text-xs font-semibold uppercase tracking-[0.18em] text-teal-700">Creator</p>
                    ) : null}
                    {user?.userId === member.userId ? (
                      <p className="mt-2 text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">You</p>
                    ) : null}
                  </div>

                  {group?.createdBy !== member.userId ? (
                    <button
                      type="button"
                      onClick={() => handleRemoveMember(member)}
                      disabled={removingMemberId === member.userId}
                      className="rounded-full border border-rose-200 bg-white px-4 py-2 text-sm font-medium text-rose-700 transition hover:bg-rose-50 disabled:cursor-not-allowed disabled:opacity-60"
                    >
                      {removingMemberId === member.userId ? 'Removing...' : 'Remove'}
                    </button>
                  ) : null}
                </div>
              </div>
            ))}
          </div>
        </SectionCard>

        <SectionCard title="Settlement summary" subtitle="Use these transactions to settle the group with as few payments as possible.">
          <SettlementList settlements={settlements} membersById={membersById} />
        </SectionCard>
      </div>

      <SectionCard
        title="Pending requests"
        subtitle="These people will only join after they approve the request from their own account."
      >
        <JoinRequestList
          requests={joinRequests.filter((request) => request.status === 'pending')}
          emptyMessage="No pending join requests for this group."
        />
      </SectionCard>

      <div className="grid gap-8 xl:grid-cols-[1.1fr_0.9fr]">
        <SectionCard title="Expenses" subtitle="A chronological record of what was paid and how each expense was split.">
          <ExpenseList expenses={expenses} membersById={membersById} onManagePayments={setSelectedExpense} />
        </SectionCard>

        <SectionCard title="Balances" subtitle="Positive balances should receive money. Negative balances need to pay.">
          <BalanceList balances={balances} />
        </SectionCard>
      </div>

      {showAddExpense && group ? (
        <AddExpenseModal
          group={group}
          onClose={() => setShowAddExpense(false)}
          onExpenseCreated={handleExpenseCreated}
        />
      ) : null}

      {showAddMember && group ? (
        <AddMemberModal
          group={group}
          joinRequests={joinRequests}
          onClose={() => setShowAddMember(false)}
          onRequestCreated={handleRequestCreated}
        />
      ) : null}

      {selectedExpense ? (
        <UpdateExpensePaymentsModal
          expense={selectedExpense}
          onClose={() => setSelectedExpense(null)}
          onPaymentsUpdated={handleExpensePaymentsUpdated}
        />
      ) : null}
    </div>
  )
}

export default GroupDetailPage
