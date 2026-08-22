import Link from 'next/link'
import { useAuth } from '@/contexts/AuthContext'
import AppLayout from '@/components/AppLayout'
import Card from '@/components/ui/Card'

function DashboardIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <line x1="18" y1="20" x2="18" y2="10" />
      <line x1="12" y1="20" x2="12" y2="4" />
      <line x1="6" y1="20" x2="6" y2="14" />
    </svg>
  )
}

function PlusIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <line x1="12" y1="5" x2="12" y2="19" />
      <line x1="5" y1="12" x2="19" y2="12" />
    </svg>
  )
}

function WalletIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M21 12V7H5a2 2 0 0 1 0-4h14v4" />
      <path d="M3 5v14a2 2 0 0 0 2 2h16v-5" />
      <path d="M18 12a2 2 0 0 0 0 4h4v-4Z" />
    </svg>
  )
}

function TagIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M20.59 13.41 11 3.83A2 2 0 0 0 9.59 3.24L3 3v6.59a2 2 0 0 0 .59 1.41l9.58 9.58a2 2 0 0 0 2.83 0l4.59-4.59a2 2 0 0 0 0-2.83Z" />
      <circle cx="7.5" cy="7.5" r="1.5" />
    </svg>
  )
}

const SHORTCUTS = [
  { href: '/dashboard', label: 'Dashboard', description: 'Ver resumo financeiro', Icon: DashboardIcon },
  { href: '/transactions/new', label: 'Novo Lançamento', description: 'Registrar receita ou despesa', Icon: PlusIcon },
  { href: '/bank-accounts', label: 'Contas Bancárias', description: 'Gerenciar suas contas', Icon: WalletIcon },
  { href: '/categories', label: 'Categorias', description: 'Organizar seu plano de contas', Icon: TagIcon },
]

function ShortcutCard({ href, label, description, Icon }) {
  return (
    <Link href={href}>
      <Card className="h-full transition-shadow hover:shadow-md cursor-pointer">
        <div className="h-12 w-12 rounded-full bg-primary-soft text-primary flex items-center justify-center mb-4">
          <Icon className="h-6 w-6" />
        </div>
        <p className="font-heading font-semibold text-lg text-text-primary">{label}</p>
        <p className="text-sm text-text-secondary mt-1">{description}</p>
      </Card>
    </Link>
  )
}

export default function HomePage() {
  // AppLayout já cuida do spinner de "verificando autenticação" e do redirect pra
  // /login quando não há usuário — aqui só evitamos usar user.name antes dele existir.
  const { user } = useAuth()

  return (
    <AppLayout>
      {user && (
        <>
          <h1 className="text-3xl font-heading font-semibold mb-1">Olá, {user.name}</h1>
          <p className="text-text-secondary mb-8">O que você quer fazer agora?</p>

          <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
            {SHORTCUTS.map(shortcut => <ShortcutCard key={shortcut.href} {...shortcut} />)}
          </div>
        </>
      )}
    </AppLayout>
  )
}
