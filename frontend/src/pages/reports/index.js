import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import Card from '@/components/ui/Card'

export default function ReportsPage() {
  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Relatórios</h1>
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <Link href="/reports/dre">
          <Card className="h-full hover:shadow-soft transition-shadow">
            <p className="font-medium mb-1">DRE — Demonstrativo de Resultado</p>
            <p className="text-sm text-text-secondary">Receitas e despesas por categoria, mês a mês, ao longo de um ano.</p>
          </Card>
        </Link>
        <Link href="/reports/pending">
          <Card className="h-full hover:shadow-soft transition-shadow">
            <p className="font-medium mb-1">Contas a Pagar / Receber</p>
            <p className="text-sm text-text-secondary">Todos os lançamentos pendentes, separados por tipo, com vencidos destacados.</p>
          </Card>
        </Link>
      </div>
    </AppLayout>
  )
}
