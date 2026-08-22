import Link from 'next/link'
import { useState } from 'react'
import { useRouter } from 'next/router'
import { useAuth } from '@/contexts/AuthContext'
import Spinner from '@/components/ui/Spinner'

export default function AppLayout({ children }) {
  const router = useRouter()
  const { user, loading, logout } = useAuth()
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const links = [
    { href: '/dashboard', label: 'Dashboard' },
    { href: '/transactions', label: 'Lançamentos' },
    { href: '/categories', label: 'Categorias' },
    { href: '/category-budgets', label: 'Orçamentos' },
    { href: '/bank-accounts', label: 'Contas Bancárias' },
  ]

  if (loading) {
    return (
      <div className="p-8 flex items-center gap-2 text-sm text-text-secondary">
        <Spinner className="h-4 w-4" /> Verificando autenticação...
      </div>
    )
  }

  if (!user) {
    router.push('/login')
    return null
  }

  return (
    <div>
      <div className="md:hidden flex items-center justify-between px-4 py-3 bg-surface border-b border-border">
        <span className="flex items-center gap-2 text-base font-heading font-semibold text-primary">
          <img src="/favicon.png" alt="" className="h-6 w-6" />
          Semeia Grana
        </span>
        <button
          onClick={() => setSidebarOpen(!sidebarOpen)}
          aria-label={sidebarOpen ? 'Fechar menu' : 'Abrir menu'}
          className="text-2xl leading-none px-1"
        >
          ☰
        </button>
      </div>

      <div className="flex">
        {/* No mobile o menu é uma sobreposição (fixed) com um fundo escurecido —
            antes ele "empurrava" o conteúdo no layout flex, espremendo a tela
            principal em telas pequenas quando aberto. */}
        {sidebarOpen && (
          <div
            className="fixed inset-0 z-40 bg-black/40 md:hidden"
            onClick={() => setSidebarOpen(false)}
            aria-hidden="true"
          />
        )}

        <aside
          className={`fixed inset-y-0 left-0 z-50 w-64 bg-panel border-r border-border px-4 py-6 transform transition-transform duration-200 ease-out
            ${sidebarOpen ? 'translate-x-0' : '-translate-x-full'}
            md:static md:translate-x-0 md:min-h-screen md:block`}
        >
          <div className="flex items-center gap-2 text-lg font-heading font-semibold text-primary mb-6">
            <img src="/favicon.png" alt="" className="h-7 w-7" />
            Semeia Grana
          </div>
          <nav className="flex flex-col gap-2">
            {links.map(link => (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setSidebarOpen(false)}
                className={`px-3 py-2 rounded-md text-sm transition-colors ${router.pathname.startsWith(link.href)
                  ? 'bg-primary-soft text-primary font-semibold'
                  : 'text-text-secondary hover:text-text-primary'
                  }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>
        </aside>

        <main className="flex-1 min-w-0 px-4 sm:px-6 py-6 sm:py-8">
          <header className="flex justify-between items-center gap-3 mb-6 sm:mb-8 pb-4 border-b border-border">
            <span className="text-sm text-text-secondary truncate">Olá, {user?.name}</span>
            <button onClick={logout} className="text-sm text-text-secondary hover:text-text-primary shrink-0">
              Sair
            </button>
          </header>
          {children}
        </main>
      </div>
    </div>
  )
}
