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
import AppLayout from '@/components/AppLayout'
import { dashboardService } from '@/services/dashboardService'
import Card from '@/components/ui/Card'
import FormSelect from '@/components/FormSelect'

const MONTHS = [
  'Janeiro', 'Fevereiro', 'Março', 'Abril', 'Maio', 'Junho',
  'Julho', 'Agosto', 'Setembro', 'Outubro', 'Novembro', 'Dezembro',
]

const INCOME_COLOR = '#1E7A46'
const EXPENSE_COLOR = '#B3261E'
const AXIS_COLOR = '#898781'
const GRID_COLOR = '#E5E3DC'
const LABEL_COLOR = '#6B6960'

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

export default function DashboardPage() {
  const now = new Date()
  const [year, setYear] = useState(String(now.getFullYear()))
  const [month, setMonth] = useState(String(now.getMonth() + 1))
  const [summary, setSummary] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    let cancelled = false
    setLoading(true)
    dashboardService.getMonthlySummary({ year, month })
      .then(res => { if (!cancelled) setSummary(res.data) })
      .catch(() => { if (!cancelled) alert('Falha ao carregar o resumo do mês') })
      .finally(() => { if (!cancelled) setLoading(false) })
    return () => { cancelled = true }
  }, [year, month])

  const chartData = (summary?.categoryBreakdown ?? [])
    .map(b => ({
      name: b.categoryGroupName,
      type: b.type,
      value: b.type === 'Expense' ? -b.total : b.total,
    }))
    .sort((a, b) => b.value - a.value)

  const chartHeight = Math.max(160, chartData.length * 48)

  return (
    <AppLayout>
      <div className="flex justify-between items-center mb-4">
        <h1 className="text-2xl font-semibold">Dashboard</h1>
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

      {!loading && summary && (
        <>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <Card>
              <p className="text-sm text-text-secondary mb-1">Receitas do mês</p>
              <p className="text-2xl font-semibold text-income">{formatCurrency(summary.totalIncome)}</p>
            </Card>
            <Card>
              <p className="text-sm text-text-secondary mb-1">Despesas do mês</p>
              <p className="text-2xl font-semibold text-expense">{formatCurrency(summary.totalExpense)}</p>
            </Card>
            <Card>
              <p className="text-sm text-text-secondary mb-1">Saldo do mês</p>
              <p className={`text-2xl font-semibold ${summary.balance >= 0 ? 'text-income' : 'text-expense'}`}>
                {formatCurrency(summary.balance)}
              </p>
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
        </>
      )}
    </AppLayout>
  )
}
