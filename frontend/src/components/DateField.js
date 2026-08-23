function formatDateDisplay(iso) {
  if (!iso) return ''
  const [y, m, d] = iso.split('-')
  return `${d}/${m}/${y}`
}

function CalendarIcon({ className }) {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={className}>
      <rect x="3" y="4" width="18" height="18" rx="2" />
      <path d="M16 2v4M8 2v4M3 10h18" />
    </svg>
  )
}

// O <input type=date> nativo é quem abre o seletor de data do sistema (é por isso que
// continua aqui), mas no Safari real ele mesmo decide como desenhar o valor (no iOS,
// por extenso — "31 de ago. de 2026") sem respeitar de forma confiável a largura/o
// arredondamento definidos por CSS, e nenhuma tentativa de conter isso por fora
// (min-width, overflow-hidden, clip-path) segurou de forma consistente em dispositivo
// real. Solução: o input fica funcional porém 100% invisível (opacity-0, cobrindo toda
// a área via peer), e quem aparece de verdade é este <div>, com o texto formatado por
// nós — que não tem nenhuma dessas inconsistências por ser HTML comum, não um widget
// nativo do sistema.
export default function DateField({ label, value, onChange, error, min, max }) {
  return (
    <div className="mb-4">
      {label && <label className="block text-sm text-text-secondary mb-1">{label}</label>}
      <div className="relative">
        <input
          type="date"
          value={value || ''}
          onChange={e => onChange(e.target.value)}
          min={min}
          max={max}
          className="peer absolute inset-0 w-full h-full opacity-0 cursor-pointer"
        />
        <div className="w-full border border-border rounded-md pl-3 pr-9 py-2 text-base pointer-events-none bg-surface peer-focus:ring-2 peer-focus:ring-primary">
          {value ? formatDateDisplay(value) : <span className="text-text-muted">dd/mm/aaaa</span>}
        </div>
        <CalendarIcon className="pointer-events-none absolute right-3 top-1/2 -translate-y-1/2 h-4 w-4 text-text-secondary" />
      </div>
      {error && <div className="text-red-600 text-sm mt-1">{error}</div>}
    </div>
  )
}
