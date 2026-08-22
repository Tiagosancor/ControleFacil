import { useEffect, useState } from 'react'
import {
  Bar,
  BarChart,
  CartesianGrid,
  ReferenceLine,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'
import Link from 'next/link'
import AppLayout from '@/components/AppLayout'
import { dashboardService } from '@/services/dashboardService'
import { categoryBudgetService } from '@/services/categoryBudgetService'
import Card from '@/components/ui/Card'
import FormSelect from '@/components/FormSelect'
import Skeleton from '@/components/ui/Skeleton'
import BudgetProgressBar from '@/components/BudgetProgressBar'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const INCOME_COLOR = '#8A6A1B'
const EXPENSE_COLOR = '#A6503B'
const AXIS_COLOR = '#8B978F'
const GRID_COLOR = '#CBD5CE'
const LABEL_COLOR = '#4B5A52'

function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

// Espaço reservado ao eixo Y (largura 170 + margem esquerda 8, ver <YAxis>/<BarChart>
// abaixo): um rótulo à esquerda da barra não pode invadir esse espaço.
const Y_AXIS_BOUNDARY = 182

// Barra + rótulo desenhados juntos: a geometria (x/width) que o Recharts entrega
// aqui é a do retângulo real, então o rótulo sempre acerta a ponta da barra —
// diferente do LabelList, cuja geometria calculada não bate para barras negativas.
// Para valores negativos o Recharts entrega x fixo na baseline e width negativo
// (inclusive durante a animação de entrada), então normalizamos com min/abs em
// vez de assumir que x já é a borda esquerda.
function DivergingBar(props) {
  const { x, y, width, height, value, payload } = props
  const left = Math.min(x, x + width)
  const barWidth = Math.abs(width)
  const isPositive = value >= 0
  const fill = payload.type === 'Income' ? INCOME_COLOR : EXPENSE_COLOR
  const label = formatCurrency(Math.abs(value))
  const estimatedLabelWidth = label.length * 6.5

  // "Medir antes de posicionar": rótulo fora da barra quando cabe; se a barra é
  // grande demais para caber fora (perto do eixo Y), o rótulo vai para dentro
  // dela (branco, encostado na base); se nem isso couber, omite — o valor
  // continua disponível no tooltip.
  let labelX = null
  let anchor = 'start'
  let color = LABEL_COLOR
  if (isPositive) {
    labelX = left + barWidth + 6
    anchor = 'start'
  } else if (left - 6 - estimatedLabelWidth > Y_AXIS_BOUNDARY) {
    labelX = left - 6
    anchor = 'end'
  } else if (barWidth > estimatedLabelWidth + 12) {
    labelX = left + barWidth - 6
    anchor = 'end'
    color = '#FFFFFF'
  }

  return (
    <g>
      <rect x={left} y={y} width={barWidth} height={height} fill={fill} rx={2} ry={2} />
      {labelX !== null && (
        <text x={labelX} y={y + height / 2} dy={4} textAnchor={anchor} fontSize={12} fill={color}>
          {label}
        </text>
      )}
    </g>
  )
}

function ChartTooltip({ active, payload }) {
  if (!active || !payload?.length) return null
  const { name, type, value } = payload[0].payload
  return (
    <div className="bg-surface border border-border rounded-md px-3 py-2 text-sm shadow-sm">
      <p className="font-medium text-text-primary">{name}</p>
      <p className={type === 'Income' ? 'text-income' : 'text-expense'}>
        {type === 'Income' ? 'Receita' : 'Despesa'}: {formatCurrency(Math.abs(value))}
      </p>
    </div>
  )
}

function formatCompactCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL', notation: 'compact', maximumFractionDigits: 1 })
}

function HistoricalTooltip({ active, payload, label }) {
  if (!active || !payload?.length) return null
  return (
    <div className="bg-surface border border-border rounded-md px-3 py-2 text-sm shadow-sm">
      <p className="font-medium text-text-primary mb-1">{label}</p>
      {payload.map(p => (
        <p key={p.dataKey} className={p.dataKey === 'income' ? 'text-income' : 'text-expense'}>
          {p.dataKey === 'income' ? 'Receita' : 'Despesa'}: {formatCurrency(p.value)}
        </p>
      ))}
    </div>
  )
}

function formatDueDate(iso) {
  const [y, m, d] = iso.split('-')
  return `${d}/${m}/${y}`
}

function formatPercent(value) {
  const sign = value > 0 ? '+' : ''
  return `${sign}${value.toFixed(1).replace('.', ',')}%`
}

// Pra despesa, ficar ACIMA da meta é ruim (desvio positivo = vermelho); pra receita e
// lucro é o contrário (desvio positivo = verde). deviationPercent vem null quando a
// meta é zero/negativa (ver comentário no backend) — nesse caso não mostra nada em vez
// de um percentual sem leitura confiável.
function GoalDeviation({ comparison, invert = false }) {
  if (!comparison || comparison.deviationPercent === null) return null
  const dev = comparison.deviationPercent
  const isGood = invert ? dev <= 0 : dev >= 0
  return (
    <p className={`text-xs mt-1 ${isGood ? 'text-primary' : 'text-terracotta'}`}>
      Meta: {formatCurrency(comparison.goal)} · {formatPercent(dev)}
    </p>
  )
}

export default function DashboardPage() {
  const now = new Date()
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [summary, setSummary] = useState(null)
  const [loading, setLoading] = useState(true)
  const [dueSoon, setDueSoon] = useState([])
  const [budgets, setBudgets] = useState([])
  const [goalComparison, setGoalComparison] = useState(null)
  const [historical, setHistorical] = useState([])
  const [analyses, setAnalyses] = useState([])

  const loadSummary = () => {
    setLoading(true)
    return dashboardService.getMonthlySummary({ year, month })
      .then(res => setSummary(res.data))
      .catch(() => alert('Falha ao carregar o resumo do mês'))
      .finally(() => setLoading(false))
  }
  const loadBudgets = () => categoryBudgetService.list({ year, month }).then(res => setBudgets(res.data)).catch(() => {})
  const loadDueSoon = () => dashboardService.getDueSoon().then(res => setDueSoon(res.data)).catch(() => {})
  const loadGoalComparison = () => dashboardService.getGoalComparison({ year, month }).then(res => setGoalComparison(res.data)).catch(() => setGoalComparison(null))
  const loadHistorical = () => dashboardService.getHistorical({ year, month, monthsBack: 3 }).then(res => setHistorical(res.data)).catch(() => {})
  const loadAnalyses = () => dashboardService.getAnalyses({ year, month }).then(res => setAnalyses(res.data)).catch(() => {})

  useEffect(() => { loadSummary() }, [year, month])
  useEffect(() => { loadBudgets() }, [year, month])
  useEffect(() => { loadGoalComparison() }, [year, month])
  useEffect(() => { loadHistorical() }, [year, month])
  useEffect(() => { loadAnalyses() }, [year, month])
  // Independente do filtro de Ano/Mês acima — "vencendo em breve" é sempre relativo a
  // hoje, não ao período selecionado no resumo do mês.
  useEffect(() => { loadDueSoon() }, [])

  // Registro rápido (Sprint H, FAB no AppLayout) cria numa página qualquer — se for
  // esta, atualiza tudo que depende de lançamentos sozinho.
  useEffect(() => {
    const handler = () => {
      loadSummary(); loadBudgets(); loadDueSoon()
      loadGoalComparison(); loadHistorical(); loadAnalyses()
    }
    window.addEventListener('semeiagrana:transaction-created', handler)
    return () => window.removeEventListener('semeiagrana:transaction-created', handler)
  }, [year, month])

  const chartData = (summary?.categoryBreakdown ?? [])
    .map(b => ({
      name: b.categoryGroupName,
      type: b.type,
      value: b.type === 'Expense' ? -b.total : b.total,
    }))
    .sort((a, b) => b.value - a.value)

  const historicalData = historical.map(h => ({
    name: `${MONTHS[h.month - 1].slice(0, 3)}/${String(h.year).slice(2)}`,
    income: h.totalIncome,
    expense: h.totalExpense,
  }))

  const chartHeight = Math.max(160, chartData.length * 48)

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-heading font-semibold">Dashboard</h1>
      </div>

      <Card className="mb-6">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <FormSelect label="Ano" value={year} onChange={setYear}>
            {[now.getFullYear() - 1, now.getFullYear(), now.getFullYear() + 1].map(y => (
              <option key={y} value={y}>{y}</option>
            ))}
          </FormSelect>
          <FormSelect label="Mês" value={month} onChange={setMonth}>
            {MONTHS.map((m, i) => <option key={m} value={i + 1}>{m}</option>)}
          </FormSelect>
        </div>
      </Card>

      {dueSoon.length > 0 && (
        <Card className="mb-6 bg-gold-wash border-gold/30">
          <p className="font-semibold text-gold mb-2">
            {dueSoon.length} lançamento(s) pendente(s) vencendo em breve
          </p>
          <ul className="space-y-1">
            {dueSoon.map(item => (
              <li key={item.transactionId} className="flex justify-between gap-4 text-sm">
                <span className="text-text-primary truncate">
                  {item.description}
                  {item.isOverdue && <span className="text-terracotta font-medium"> · vencido</span>}
                </span>
                <span className="text-text-secondary tabular-nums shrink-0">
                  {formatCurrency(item.amount)} · {formatDueDate(item.entryDate)}
                </span>
              </li>
            ))}
          </ul>
        </Card>
      )}

      {loading && (
        <>
          <Card className="mb-6">
            <Skeleton className="h-4 w-32 mb-2" />
            <Skeleton className="h-8 w-40" />
          </Card>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            {Array.from({ length: 3 }).map((_, i) => (
              <Card key={i}>
                <Skeleton className="h-4 w-24 mb-2" />
                <Skeleton className="h-7 w-32" />
              </Card>
            ))}
          </div>
          <Card>
            <Skeleton className="h-4 w-48 mb-4" />
            <Skeleton className="h-40 w-full" />
          </Card>
        </>
      )}

      {!loading && summary && (
        <>
          <Card className="mb-6 bg-gradient-to-b from-primary-soft to-surface">
            <p className="text-sm text-text-secondary mb-1">Saldo total</p>
            <p className={`text-4xl font-heading font-medium tabular-nums ${summary.totalBalance >= 0 ? 'text-primary' : 'text-expense'}`}>
              {formatCurrency(summary.totalBalance)}
            </p>
            <p className="text-xs text-text-secondary mt-1">
              Saldo inicial das contas + lançamentos já pagos, sem limitar ao mês selecionado
            </p>
          </Card>

          <div className="flex justify-between items-center mb-2">
            <p className="text-sm text-text-secondary">Resumo do mês {goalComparison && '(Realizado x Meta)'}</p>
            {!goalComparison && <Link href="/monthly-goals" className="text-xs text-link">Definir meta</Link>}
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <Card>
              <p className="text-sm text-text-secondary mb-1">Receitas do mês</p>
              <p className="text-2xl font-heading font-semibold tabular-nums text-income">{formatCurrency(summary.totalIncome)}</p>
              <GoalDeviation comparison={goalComparison?.income} />
            </Card>
            <Card>
              <p className="text-sm text-text-secondary mb-1">Despesas do mês</p>
              <p className="text-2xl font-heading font-semibold tabular-nums text-expense">{formatCurrency(summary.totalExpense)}</p>
              <GoalDeviation comparison={goalComparison?.expense} invert />
            </Card>
            <Card>
              <p className="text-sm text-text-secondary mb-1">Saldo do mês</p>
              <p className={`text-2xl font-heading font-semibold tabular-nums ${summary.balance >= 0 ? 'text-primary' : 'text-expense'}`}>
                {formatCurrency(summary.balance)}
              </p>
              <GoalDeviation comparison={goalComparison?.profit} />
            </Card>
          </div>

          <Card>
            <p className="text-sm text-text-secondary mb-4">Receitas e despesas por grupo</p>
            {chartData.length === 0 ? (
              <p className="text-sm text-text-secondary py-8 text-center">Nenhum lançamento neste mês.</p>
            ) : (
              <ResponsiveContainer width="100%" height={chartHeight}>
                <BarChart data={chartData} layout="vertical" margin={{ top: 4, right: 48, bottom: 4, left: 8 }}>
                  <CartesianGrid horizontal={false} stroke={GRID_COLOR} />
                  <XAxis
                    type="number"
                    tickFormatter={v => formatCurrency(Math.abs(v))}
                    tick={{ fontSize: 11, fill: AXIS_COLOR }}
                    axisLine={{ stroke: GRID_COLOR }}
                    tickLine={false}
                  />
                  <YAxis
                    type="category"
                    dataKey="name"
                    width={170}
                    tick={{ fontSize: 12, fill: AXIS_COLOR }}
                    axisLine={{ stroke: GRID_COLOR }}
                    tickLine={false}
                  />
                  <ReferenceLine x={0} stroke={GRID_COLOR} />
                  <Tooltip content={<ChartTooltip />} cursor={{ fill: 'rgba(0,0,0,0.03)' }} />
                  <Bar dataKey="value" barSize={20} shape={<DivergingBar />} />
                </BarChart>
              </ResponsiveContainer>
            )}
          </Card>

          {historicalData.length > 0 && (
            <Card className="mt-6">
              <div className="flex items-center justify-between mb-4">
                <p className="text-sm text-text-secondary">Comparativo histórico (últimos {historicalData.length} meses)</p>
                <div className="flex items-center gap-3 text-xs text-text-secondary">
                  <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full bg-income inline-block" />Receita</span>
                  <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full bg-expense inline-block" />Despesa</span>
                </div>
              </div>
              {/* key força remount a cada troca de Ano/Mês: o <Bar> padrão do Recharts (ao
                  contrário do shape customizado do gráfico acima) não redesenha as barras
                  de forma confiável quando só o data prop muda — confirmado visualmente
                  (eixo X atualizava, barras ficavam com altura zero). Remount contorna. */}
              <ResponsiveContainer key={`${year}-${month}`} width="100%" height={220}>
                <BarChart data={historicalData} margin={{ top: 4, right: 8, bottom: 4, left: 0 }}>
                  <CartesianGrid vertical={false} stroke={GRID_COLOR} />
                  <XAxis dataKey="name" tick={{ fontSize: 12, fill: AXIS_COLOR }} axisLine={{ stroke: GRID_COLOR }} tickLine={false} />
                  <YAxis
                    tickFormatter={formatCompactCurrency}
                    tick={{ fontSize: 11, fill: AXIS_COLOR }}
                    axisLine={{ stroke: GRID_COLOR }}
                    tickLine={false}
                    width={64}
                  />
                  <Tooltip content={<HistoricalTooltip />} cursor={{ fill: 'rgba(0,0,0,0.03)' }} />
                  <Bar dataKey="income" fill={INCOME_COLOR} radius={[2, 2, 0, 0]} isAnimationActive={false} />
                  <Bar dataKey="expense" fill={EXPENSE_COLOR} radius={[2, 2, 0, 0]} isAnimationActive={false} />
                </BarChart>
              </ResponsiveContainer>
            </Card>
          )}

          {analyses.length > 0 && (
            <Card className="mt-6">
              <p className="text-sm text-text-secondary mb-4">Análises automáticas</p>
              <ul className="flex flex-col gap-2">
                {analyses.map((a, i) => (
                  <li
                    key={i}
                    className={`text-sm flex items-start gap-2 ${a.type === 'positive' ? 'text-primary' : a.type === 'warning' ? 'text-gold' : 'text-terracotta'
                      }`}
                  >
                    <span className="h-1.5 w-1.5 rounded-full bg-current mt-1.5 shrink-0" aria-hidden="true" />
                    {a.message}
                  </li>
                ))}
              </ul>
            </Card>
          )}

          {budgets.length > 0 && (
            <Card className="mt-6">
              <div className="flex justify-between items-center mb-4">
                <p className="text-sm text-text-secondary">Orçamento por categoria</p>
                <Link href="/category-budgets" className="text-xs text-link">Gerenciar</Link>
              </div>
              <div className="flex flex-col gap-4">
                {budgets.map(budget => (
                  <BudgetProgressBar
                    key={budget.id}
                    categoryName={budget.categoryName}
                    spent={budget.spent}
                    limitAmount={budget.limitAmount}
                    percentage={budget.percentage}
                  />
                ))}
              </div>
            </Card>
          )}
        </>
      )}
    </AppLayout>
  )
}
