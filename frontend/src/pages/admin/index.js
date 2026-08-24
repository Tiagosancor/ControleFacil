import { useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import AppLayout from '@/components/AppLayout'
import { useAuth } from '@/contexts/AuthContext'
import { usageEventService } from '@/services/usageEventService'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import Skeleton from '@/components/ui/Skeleton'

const PAGE_SIZE = 20

function formatDateTime(value) {
  return new Date(value).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' })
}

export default function AdminPage() {
  const router = useRouter()
  const { loading: authLoading, isAdmin } = useAuth()

  const [loggedInUsers, setLoggedInUsers] = useState([])
  const [loadingLoggedIn, setLoadingLoggedIn] = useState(true)

  const [history, setHistory] = useState({ items: [], total: 0 })
  const [page, setPage] = useState(1)
  const [loadingHistory, setLoadingHistory] = useState(true)

  // Guarda de acesso: só decide depois que o AuthContext termina de resolver o
  // usuário — antes disso isAdmin ainda é undefined/false por padrão, e um
  // redirect prematuro chutaria até admin de verdade pra fora da página.
  useEffect(() => {
    if (!authLoading && !isAdmin) {
      router.push('/')
    }
  }, [authLoading, isAdmin, router])

  useEffect(() => {
    if (!isAdmin) return
    setLoadingLoggedIn(true)
    usageEventService.getLoggedInUsers()
      .then(res => setLoggedInUsers(res.data))
      .catch(() => alert('Falha ao carregar usuários logados'))
      .finally(() => setLoadingLoggedIn(false))
  }, [isAdmin])

  useEffect(() => {
    if (!isAdmin) return
    setLoadingHistory(true)
    usageEventService.getLoginHistory({ page, pageSize: PAGE_SIZE })
      .then(res => setHistory(res.data))
      .catch(() => alert('Falha ao carregar histórico de login'))
      .finally(() => setLoadingHistory(false))
  }, [isAdmin, page])

  if (authLoading || !isAdmin) {
    return null
  }

  const totalPages = Math.max(1, Math.ceil(history.total / PAGE_SIZE))

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Administração</h1>

      <Card className="mb-6">
        <h2 className="text-lg font-heading font-medium mb-1">Usuários logados agora</h2>
        <p className="text-sm text-text-secondary mb-4">
          Aproximação: usuários com login registrado dentro da janela de validade do token.
        </p>
        {loadingLoggedIn ? (
          <div className="flex flex-col gap-2">
            {Array.from({ length: 3 }).map((_, i) => <Skeleton key={i} className="h-10 w-full" />)}
          </div>
        ) : loggedInUsers.length === 0 ? (
          <p className="text-sm text-text-secondary">Nenhum usuário logado no momento.</p>
        ) : (
          <div className="flex flex-col gap-2">
            {loggedInUsers.map(u => (
              <div key={u.userId} className="flex justify-between items-center gap-2 text-sm py-1">
                <div className="min-w-0">
                  <p className="font-medium truncate">{u.userName}</p>
                  <p className="text-text-secondary truncate">{u.userEmail}</p>
                </div>
                <p className="text-text-secondary shrink-0 tabular-nums">{formatDateTime(u.lastLoginAt)}</p>
              </div>
            ))}
          </div>
        )}
      </Card>

      <Card>
        <h2 className="text-lg font-heading font-medium mb-4">Histórico de login</h2>
        {loadingHistory ? (
          <div className="flex flex-col gap-2">
            {Array.from({ length: 6 }).map((_, i) => <Skeleton key={i} className="h-10 w-full" />)}
          </div>
        ) : history.items.length === 0 ? (
          <p className="text-sm text-text-secondary">Nenhum login registrado.</p>
        ) : (
          <>
            <div className="overflow-x-auto">
              <table className="w-full text-sm">
                <thead>
                  <tr className="text-xs text-text-secondary border-b border-border">
                    <th className="text-left p-2 font-normal">Usuário</th>
                    <th className="text-left p-2 font-normal">Email</th>
                    <th className="text-left p-2 font-normal">Data/hora</th>
                  </tr>
                </thead>
                <tbody>
                  {history.items.map((item, i) => (
                    <tr key={i} className="border-b border-border">
                      <td className="p-2">{item.userName}</td>
                      <td className="p-2 text-text-secondary">{item.userEmail}</td>
                      <td className="p-2 tabular-nums">{formatDateTime(item.createdAt)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
            <div className="flex justify-between items-center mt-4">
              <p className="text-sm text-text-secondary">Página {page} de {totalPages} · {history.total} registro(s)</p>
              <div className="flex gap-2">
                <Button variant="secondary" disabled={page <= 1} onClick={() => setPage(p => p - 1)}>Anterior</Button>
                <Button variant="secondary" disabled={page >= totalPages} onClick={() => setPage(p => p + 1)}>Próxima</Button>
              </div>
            </div>
          </>
        )}
      </Card>
    </AppLayout>
  )
}
