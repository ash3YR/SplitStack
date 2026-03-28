import { Link } from 'react-router-dom'

function GroupCard({ group }) {
  const memberCount = group.members?.length ?? group.memberCount ?? 0

  return (
    <Link
      to={`/groups/${group.id}`}
      className="group rounded-[1.75rem] border border-slate-200/70 bg-white p-5 shadow-sm transition hover:-translate-y-1 hover:border-teal-200 hover:shadow-xl hover:shadow-teal-500/10"
    >
      <div className="flex items-start justify-between gap-3">
        <div>
          <p className="text-xs font-semibold uppercase tracking-[0.22em] text-teal-600">Group</p>
          <h3 className="mt-2 text-xl font-semibold text-slate-900">{group.name}</h3>
        </div>
        <span className="rounded-full bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700">
          {memberCount} members
        </span>
      </div>
      <p className="mt-4 text-sm text-slate-600">
        Open this workspace to review expenses, balances, and settlement suggestions.
      </p>
      <div className="mt-6 flex items-center justify-between text-sm font-medium text-slate-500">
        <span>View details</span>
        <span className="transition group-hover:translate-x-1">→</span>
      </div>
    </Link>
  )
}

export default GroupCard
