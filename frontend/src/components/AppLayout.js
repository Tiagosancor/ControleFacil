import Link from 'next/link'
import { useState } from 'react'
import { useRouter } from 'next/router'
import { useAuth } from '@/contexts/AuthContext'

export default function AppLayout({ children }) {
  const router = useRouter()
  const { user, loading, logout } = useAuth()
  const [sidebarOpen, setSidebarOpen] = useState(false)

  const links = [
    { href: '/transactions', label: 'Lançamentos' },
    { href: '/categories', label: 'Categorias' },
    { href: '/bank-accounts', label: 'Contas Bancárias' },
  ]

  if (loading) return <div className="p-8 text-sm text-text-secondary">Verificando autenticação...</div>

  if (!user) {
    router.push('/login')
    return null
  }

  return (
    <div>
      <div className="md:hidden flex items-center px-4 py-3 bg-surface border-b border-border">
        <button onClick={() => setSidebarOpen(!sidebarOpen)}>☰</button>
      </div>

      <div className="flex">
        <aside className={`w-64 min-h-screen bg-surface border-r border-border px-4 py-6
          ${sidebarOpen ? 'block' : 'hidden'} md:block`}>
          <div className="text-lg font-semibold text-accent mb-6">ControleFacil</div>
          <nav className="flex flex-col gap-2">
            {links.map(link => (
              <Link
                key={link.href}
                href={link.href}
                onClick={() => setSidebarOpen(false)}
                className={`px-3 py-2 rounded-md text-sm ${router.pathname.startsWith(link.href)
                  ? 'text-accent font-medium'
                  : 'text-text-secondary hover:text-text-primary'
                  }`}
              >
                {link.label}
              </Link>
            ))}
          </nav>
        </aside>

        <main className="flex-1 px-6 py-8">
          <header className="flex justify-between items-center mb-8 pb-4 border-b border-border">
            <span className="text-sm text-text-secondary">Olá, {user?.name}</span>
            <button onClick={logout} className="text-sm text-text-secondary hover:text-text-primary">
              Sair
            </button>
          </header>
          {children}
        </main>
      </div>
    </div>
  )
}
