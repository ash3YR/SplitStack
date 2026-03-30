function JoinRequestList({
  requests,
  emptyMessage,
  actionLabel,
  secondaryActionLabel,
  onAction,
  onSecondaryAction,
  loadingRequestId,
  actionVariant = 'primary',
}) {
  if (!requests.length) {
    return (
      <div className="rounded-2xl border border-dashed border-slate-300 bg-slate-50 px-4 py-6 text-sm text-slate-500">
        {emptyMessage}
      </div>
    )
  }

  return (
    <div className="space-y-3">
      {requests.map((request) => (
        <div
          key={request.id}
          className="rounded-[1.5rem] border border-slate-200 bg-slate-50 px-4 py-4"
        >
          <div className="flex flex-wrap items-start justify-between gap-3">
            <div>
              <p className="font-semibold text-slate-900">{request.groupName}</p>
              <p className="text-sm text-slate-500">
                {request.requestedByName}
                {' '}
                invited
                {' '}
                {request.targetUserName}
              </p>
              <p className="mt-1 text-xs font-semibold uppercase tracking-[0.18em] text-slate-500">
                {request.status}
              </p>
            </div>

            {onAction ? (
              <div className="flex flex-wrap gap-2">
                {onSecondaryAction ? (
                  <button
                    type="button"
                    onClick={() => onSecondaryAction(request)}
                    disabled={loadingRequestId === request.id}
                    className="rounded-full border border-slate-200 bg-white px-4 py-2 text-sm font-medium text-slate-700 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    {loadingRequestId === request.id && secondaryActionLabel ? 'Working...' : secondaryActionLabel}
                  </button>
                ) : null}
                <button
                  type="button"
                  onClick={() => onAction(request)}
                  disabled={loadingRequestId === request.id}
                  className={`rounded-full px-4 py-2 text-sm font-medium transition disabled:cursor-not-allowed disabled:opacity-60 ${
                    actionVariant === 'primary'
                      ? 'bg-slate-900 text-white hover:bg-slate-800'
                      : 'border border-teal-200 bg-white text-teal-700 hover:bg-teal-50'
                  }`}
                >
                  {loadingRequestId === request.id ? 'Working...' : actionLabel}
                </button>
              </div>
            ) : null}
          </div>
        </div>
      ))}
    </div>
  )
}

export default JoinRequestList
