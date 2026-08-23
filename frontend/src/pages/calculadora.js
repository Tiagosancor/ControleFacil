import { useMemo, useState } from 'react'
import AppLayout from '@/components/AppLayout'
import Card from '@/components/ui/Card'
import FormInput from '@/components/FormInput'
import {
  ResponsiveContainer, AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip,
} from 'recharts'

const PRIMARY_COLOR = '#285649'
const GOLD_COLOR = '#8A6A1B'
const AXIS_COLOR = '#8B978F'
const GRID_COLOR = '#CBD5CE'

function formatCurrency(value) {
  if (!Number.isFinite(value)) return '—'
  return value.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function TabButton({ active, onClick, children }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className={`py-2 rounded-md text-sm font-medium border transition-colors ${active ? 'bg-primary-soft border-primary text-primary' : 'border-border text-text-secondary'}`}
    >
      {children}
    </button>
  )
}

function PeriodToggle({ value, onChange, options }) {
  return (
    <div className="grid grid-cols-2 gap-2 mb-4">
      {options.map(opt => (
        <TabButton key={opt.value} active={value === opt.value} onClick={() => onChange(opt.value)}>
          {opt.label}
        </TabButton>
      ))}
    </div>
  )
}

function ChartTooltip({ active, payload, label }) {
  if (!active || !payload?.length) return null
  const investido = payload.find(p => p.dataKey === 'investido')?.value ?? 0
  const juros = payload.find(p => p.dataKey === 'juros')?.value ?? 0
  return (
    <div className="bg-surface border border-border rounded-md shadow-soft px-3 py-2 text-xs">
      <p className="font-medium text-text-primary mb-1">Mês {label}</p>
      <p className="text-primary">Investido: {formatCurrency(investido)}</p>
      <p style={{ color: GOLD_COLOR }}>Juros: {formatCurrency(juros)}</p>
      <p className="text-text-secondary mt-1 pt-1 border-t border-border">Total: {formatCurrency(investido + juros)}</p>
    </div>
  )
}

// Normalização de taxa/tempo: juros simples é linear (taxa ao ano / 12), juros compostos
// exige a conversão exponencial (taxa mensal equivalente), porque o aporte é mensal —
// ver docs/reference/sprint-calculadora-juros.md seções 3 e 4.
function useSimulacaoSimples(capital, taxa, taxaPeriodo, tempo, tempoPeriodo) {
  return useMemo(() => {
    const C = Number(capital)
    const iRaw = Number(taxa)
    const tRaw = Number(tempo)
    if (!Number.isFinite(C) || C <= 0 || !Number.isFinite(iRaw) || iRaw < 0 || !Number.isFinite(tRaw) || tRaw <= 0) {
      return null
    }

    const iMensal = (taxaPeriodo === 'ano' ? iRaw / 12 : iRaw) / 100
    const tMeses = tempoPeriodo === 'ano' ? tRaw * 12 : tRaw

    const J = C * iMensal * tMeses
    const M = C + J
    return { C, J, M }
  }, [capital, taxa, taxaPeriodo, tempo, tempoPeriodo])
}

function useSimulacaoComposta(capital, taxa, taxaPeriodo, tempo, tempoPeriodo, pmt) {
  return useMemo(() => {
    const C = Number(capital)
    const iRaw = Number(taxa)
    const tRaw = Number(tempo)
    const PMT = pmt === '' ? 0 : Number(pmt)
    if (
      !Number.isFinite(C) || C <= 0
      || !Number.isFinite(iRaw) || iRaw < 0
      || !Number.isFinite(tRaw) || tRaw <= 0
      || !Number.isFinite(PMT) || PMT < 0
    ) {
      return null
    }

    const n = Math.round(tempoPeriodo === 'ano' ? tRaw * 12 : tRaw)
    const iMensalPercent = taxaPeriodo === 'ano' ? (Math.pow(1 + iRaw / 100, 1 / 12) - 1) * 100 : iRaw
    const iMensal = iMensalPercent / 100

    const montanteAte = (mes) => (
      iMensal === 0
        ? C + PMT * mes
        : C * Math.pow(1 + iMensal, mes) + PMT * ((Math.pow(1 + iMensal, mes) - 1) / iMensal)
    )

    const M = montanteAte(n)
    const totalInvestido = C + PMT * n
    const totalJuros = M - totalInvestido

    const chartData = []
    for (let mes = 0; mes <= n; mes++) {
      const investidoNoMes = C + PMT * mes
      chartData.push({
        mes,
        investido: investidoNoMes,
        juros: Math.max(montanteAte(mes) - investidoNoMes, 0),
      })
    }

    return { M, totalInvestido, totalJuros, chartData }
  }, [capital, taxa, taxaPeriodo, tempo, tempoPeriodo, pmt])
}

export default function CalculadoraPage() {
  const [tab, setTab] = useState('simples')

  const [capitalS, setCapitalS] = useState('1000')
  const [taxaS, setTaxaS] = useState('1')
  const [taxaPeriodoS, setTaxaPeriodoS] = useState('mes')
  const [tempoS, setTempoS] = useState('12')
  const [tempoPeriodoS, setTempoPeriodoS] = useState('mes')
  const simples = useSimulacaoSimples(capitalS, taxaS, taxaPeriodoS, tempoS, tempoPeriodoS)

  const [capitalC, setCapitalC] = useState('1000')
  const [taxaC, setTaxaC] = useState('1')
  const [taxaPeriodoC, setTaxaPeriodoC] = useState('mes')
  const [tempoC, setTempoC] = useState('12')
  const [tempoPeriodoC, setTempoPeriodoC] = useState('mes')
  const [pmtC, setPmtC] = useState('0')
  const compostos = useSimulacaoComposta(capitalC, taxaC, taxaPeriodoC, tempoC, tempoPeriodoC, pmtC)

  return (
    <AppLayout>
      <h1 className="text-2xl font-heading font-semibold mb-6">Calculadora de Juros</h1>

      <div className="grid grid-cols-2 gap-2 mb-6 max-w-md">
        <TabButton active={tab === 'simples'} onClick={() => setTab('simples')}>Juros Simples</TabButton>
        <TabButton active={tab === 'compostos'} onClick={() => setTab('compostos')}>Juros Compostos</TabButton>
      </div>

      {tab === 'simples' ? (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
          <Card className="max-w-lg">
            <FormInput label="Capital inicial" type="number" min="0" step="0.01" value={capitalS} onChange={setCapitalS} />

            <FormInput label="Taxa de juros (%)" type="number" min="0" step="0.01" value={taxaS} onChange={setTaxaS} />
            <PeriodToggle
              value={taxaPeriodoS}
              onChange={setTaxaPeriodoS}
              options={[{ value: 'mes', label: 'ao mês' }, { value: 'ano', label: 'ao ano' }]}
            />

            <FormInput label="Tempo" type="number" min="0" step="1" value={tempoS} onChange={setTempoS} />
            <PeriodToggle
              value={tempoPeriodoS}
              onChange={setTempoPeriodoS}
              options={[{ value: 'mes', label: 'meses' }, { value: 'ano', label: 'anos' }]}
            />
          </Card>

          <Card className="max-w-lg bg-gradient-to-b from-primary-soft to-surface">
            <p className="text-sm text-text-secondary mb-1">Montante final</p>
            <p className="text-3xl font-heading font-medium text-primary tabular-nums mb-6">
              {simples ? formatCurrency(simples.M) : '—'}
            </p>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-text-secondary">Capital inicial</p>
                <p className="font-medium tabular-nums">{simples ? formatCurrency(simples.C) : '—'}</p>
              </div>
              <div>
                <p className="text-xs text-text-secondary">Juros total</p>
                <p className="font-medium tabular-nums" style={{ color: GOLD_COLOR }}>{simples ? formatCurrency(simples.J) : '—'}</p>
              </div>
            </div>
            {!simples && (
              <p className="text-xs text-terracotta mt-4">Preencha capital, taxa e tempo com valores válidos (maiores que zero).</p>
            )}
          </Card>
        </div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">
          <Card className="max-w-lg">
            <FormInput label="Capital inicial" type="number" min="0" step="0.01" value={capitalC} onChange={setCapitalC} />

            <FormInput label="Taxa de juros (%)" type="number" min="0" step="0.01" value={taxaC} onChange={setTaxaC} />
            <PeriodToggle
              value={taxaPeriodoC}
              onChange={setTaxaPeriodoC}
              options={[{ value: 'mes', label: 'ao mês' }, { value: 'ano', label: 'ao ano' }]}
            />

            <FormInput label="Tempo" type="number" min="0" step="1" value={tempoC} onChange={setTempoC} />
            <PeriodToggle
              value={tempoPeriodoC}
              onChange={setTempoPeriodoC}
              options={[{ value: 'mes', label: 'meses' }, { value: 'ano', label: 'anos' }]}
            />

            <FormInput label="Aporte mensal (opcional)" type="number" min="0" step="0.01" value={pmtC} onChange={setPmtC} />
          </Card>

          <Card className="max-w-lg bg-gradient-to-b from-primary-soft to-surface">
            <p className="text-sm text-text-secondary mb-1">Montante final</p>
            <p className="text-3xl font-heading font-medium text-primary tabular-nums mb-6">
              {compostos ? formatCurrency(compostos.M) : '—'}
            </p>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-xs text-text-secondary">Total investido</p>
                <p className="font-medium tabular-nums">{compostos ? formatCurrency(compostos.totalInvestido) : '—'}</p>
              </div>
              <div>
                <p className="text-xs text-text-secondary">Total em juros</p>
                <p className="font-medium tabular-nums" style={{ color: GOLD_COLOR }}>{compostos ? formatCurrency(compostos.totalJuros) : '—'}</p>
              </div>
            </div>
            {!compostos && (
              <p className="text-xs text-terracotta mt-4">Preencha capital, taxa e tempo com valores válidos (maiores que zero; aporte não pode ser negativo).</p>
            )}
          </Card>

          {compostos && compostos.chartData.length > 1 && (
            <Card className="lg:col-span-2">
              <p className="text-sm text-text-secondary mb-4">Evolução do montante mês a mês</p>
              <ResponsiveContainer width="100%" height={260}>
                <AreaChart data={compostos.chartData} margin={{ top: 4, right: 8, bottom: 4, left: 0 }}>
                  <defs>
                    <linearGradient id="investidoGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor={PRIMARY_COLOR} stopOpacity={0.5} />
                      <stop offset="100%" stopColor={PRIMARY_COLOR} stopOpacity={0.05} />
                    </linearGradient>
                    <linearGradient id="jurosGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor={GOLD_COLOR} stopOpacity={0.6} />
                      <stop offset="100%" stopColor={GOLD_COLOR} stopOpacity={0.05} />
                    </linearGradient>
                  </defs>
                  <CartesianGrid vertical={false} stroke={GRID_COLOR} />
                  <XAxis
                    dataKey="mes"
                    tick={{ fontSize: 11, fill: AXIS_COLOR }}
                    axisLine={{ stroke: GRID_COLOR }}
                    tickLine={false}
                    label={{ value: 'Mês', position: 'insideBottom', offset: -2, fontSize: 11, fill: AXIS_COLOR }}
                  />
                  <YAxis
                    tickFormatter={formatCurrency}
                    tick={{ fontSize: 11, fill: AXIS_COLOR }}
                    axisLine={{ stroke: GRID_COLOR }}
                    tickLine={false}
                    width={72}
                  />
                  <Tooltip content={<ChartTooltip />} />
                  <Area type="monotone" dataKey="investido" stackId="1" stroke={PRIMARY_COLOR} fill="url(#investidoGradient)" isAnimationActive={false} />
                  <Area type="monotone" dataKey="juros" stackId="1" stroke={GOLD_COLOR} fill="url(#jurosGradient)" isAnimationActive={false} />
                </AreaChart>
              </ResponsiveContainer>
              <div className="flex items-center gap-4 text-xs text-text-secondary mt-2">
                <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full bg-primary inline-block" />Investido</span>
                <span className="flex items-center gap-1"><span className="h-2 w-2 rounded-full inline-block" style={{ backgroundColor: GOLD_COLOR }} />Juros</span>
              </div>
            </Card>
          )}
        </div>
      )}
    </AppLayout>
  )
}
