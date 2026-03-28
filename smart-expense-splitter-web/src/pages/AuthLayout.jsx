import { Link } from 'react-router-dom'

function AuthLayout({ title, subtitle, footer, children }) {
  return (
    <div className="grid min-h-[calc(100vh-8rem)] gap-8 lg:grid-cols-[1.1fr_0.9fr]">
      <section className="relative overflow-hidden rounded-[2rem] border border-white/60 bg-[linear-gradient(135deg,#0f766e_0%,#166534_100%)] p-8 text-white shadow-[0_32px_80px_-36px_rgba(15,118,110,0.65)]">
        <div className="absolute -left-10 top-10 h-36 w-36 rounded-full bg-white/10 blur-2xl" />
        <div className="absolute bottom-0 right-0 h-52 w-52 rounded-full bg-amber-300/15 blur-3xl" />
        <div className="relative z-10 flex h-full flex-col justify-between">
          <div>
            <p className="text-sm font-semibold uppercase tracking-[0.28em] text-teal-100">Smart Expense Splitter</p>
            <h1 className="mt-6 max-w-xl text-4xl font-semibold leading-tight">
              Clear group spending with balances and settlement suggestions that actually feel calm.
            </h1>
            <p className="mt-5 max-w-lg text-base text-teal-50/85">
              Track who paid, who owes, and the fastest way to settle up without scrolling through spreadsheets or chat history.
            </p>
          </div>

          <div className="grid gap-4 sm:grid-cols-3">
            <div className="rounded-2xl border border-white/10 bg-white/10 p-4 backdrop-blur-sm">
              <p className="text-sm text-teal-100">Balances</p>
              <p className="mt-2 text-xl font-semibold">Live net positions</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/10 p-4 backdrop-blur-sm">
              <p className="text-sm text-teal-100">Settlements</p>
              <p className="mt-2 text-xl font-semibold">Optimized paybacks</p>
            </div>
            <div className="rounded-2xl border border-white/10 bg-white/10 p-4 backdrop-blur-sm">
              <p className="text-sm text-teal-100">Expenses</p>
              <p className="mt-2 text-xl font-semibold">Clean shared history</p>
            </div>
          </div>
        </div>
      </section>

      <section className="flex items-center justify-center">
        <div className="w-full max-w-lg rounded-[2rem] border border-white/70 bg-white/90 p-8 shadow-[0_24px_60px_-30px_rgba(15,23,42,0.3)] backdrop-blur-xl">
          <div className="mb-8">
            <Link to="/" className="text-sm font-semibold uppercase tracking-[0.22em] text-teal-700">
              Smart Splitter
            </Link>
            <h2 className="mt-4 text-3xl font-semibold text-slate-900">{title}</h2>
            <p className="mt-2 text-sm text-slate-600">{subtitle}</p>
          </div>
          {children}
          <p className="mt-6 text-sm text-slate-500">{footer}</p>
        </div>
      </section>
    </div>
  )
}

export default AuthLayout
