function formatCurrency(value) {
  return Number(value).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })
}

function barColor(percentage) {
  if (percentage > 1) return 'bg-terracotta'
  if (percentage >= 0.8) return 'bg-gold'
  return 'bg-primary'
}

export default function BudgetProgressBar({ categoryName, spent, limitAmount, percentage }) {
  const widthPercent = Math.min(percentage, 1) * 100
  const overBudget = percentage > 1

  return (
    <div>
      <div className="flex justify-between items-baseline gap-2 mb-1">
        <span className="text-sm font-medium text-text-primary truncate">{categoryName}</span>
        <span className={`text-xs shrink-0 tabular-nums ${overBudget ? 'text-terracotta font-semibold' : 'text-text-secondary'}`}>
          {formatCurrency(spent)} / {formatCurrency(limitAmount)} ({Math.round(percentage * 100)}%)
        </span>
      </div>
      <div className="h-2 rounded-full bg-panel overflow-hidden">
        <div className={`h-full rounded-full ${barColor(percentage)}`} style={{ width: `${widthPercent}%` }} />
      </div>
      {overBudget && <p className="text-xs text-terracotta mt-1">Orçamento estourado</p>}
    </div>
  )
}
