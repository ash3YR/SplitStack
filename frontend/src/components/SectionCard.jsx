function SectionCard({ title, subtitle, action, children }) {
  return (
    <section className="rounded-[2rem] border border-white/70 bg-white/85 p-6 shadow-[0_24px_60px_-32px_rgba(15,23,42,0.35)] backdrop-blur-xl">
      <div className="mb-5 flex flex-wrap items-start justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold text-slate-900">{title}</h2>
          {subtitle ? <p className="mt-1 text-sm text-slate-600">{subtitle}</p> : null}
        </div>
        {action}
      </div>
      {children}
    </section>
  )
}

export default SectionCard
