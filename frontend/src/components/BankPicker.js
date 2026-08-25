import { useEffect, useRef, useState } from 'react'
import { bankService } from '@/services/bankService'

function BankLogo({ bank, size = 28 }) {
  const [failed, setFailed] = useState(false)
  const px = `${size}px`

  if (!bank.logoUrl || failed) {
    return (
      <span
        style={{ height: px, width: px }}
        className="shrink-0 rounded-full bg-primary-soft text-primary flex items-center justify-center text-xs font-semibold"
      >
        {bank.name.trim().charAt(0).toUpperCase()}
      </span>
    )
  }

  return (
    // eslint-disable-next-line @next/next/no-img-element
    <img
      src={bank.logoUrl}
      alt=""
      style={{ height: px, width: px }}
      className="shrink-0 rounded-full object-contain bg-surface border border-border"
      onError={() => setFailed(true)}
    />
  )
}

// Busca de banco com logo — não usa <select> nativo porque cada opção precisa mostrar
// logo + nome, não só texto. Sempre opcional: Nome (apelido da conta) continua livre e
// independente disso, ver BankAccount.BankIspb.
export default function BankPicker({ label = 'Banco (opcional)', value, onChange, error }) {
  const [query, setQuery] = useState(value?.name || '')
  const [suggestions, setSuggestions] = useState([])
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const debounceRef = useRef(null)
  const containerRef = useRef(null)

  useEffect(() => {
    setQuery(value?.name || '')
  }, [value?.ispb])

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    if (value && query === value.name) return undefined // acabou de selecionar, não rebusca

    debounceRef.current = setTimeout(() => {
      setLoading(true)
      bankService.search({ search: query || undefined })
        .then(res => setSuggestions(res.data))
        .catch(() => setSuggestions([]))
        .finally(() => setLoading(false))
    }, 300)

    return () => clearTimeout(debounceRef.current)
  }, [query])

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (containerRef.current && !containerRef.current.contains(e.target)) setOpen(false)
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const handleChange = (text) => {
    setQuery(text)
    if (value) onChange(null)
    setOpen(true)
  }

  const select = (bank) => {
    onChange(bank)
    setQuery(bank.name)
    setSuggestions([])
    setOpen(false)
  }

  const clear = () => {
    onChange(null)
    setQuery('')
    setOpen(false)
  }

  return (
    <div className="mb-4 relative" ref={containerRef}>
      {label && <label className="block text-sm text-text-secondary mb-1">{label}</label>}
      <div className="relative">
        {value && (
          <span className="absolute left-3 top-1/2 -translate-y-1/2">
            <BankLogo bank={value} size={22} />
          </span>
        )}
        <input
          type="text"
          value={query}
          onChange={e => handleChange(e.target.value)}
          onFocus={() => setOpen(true)}
          placeholder="Buscar banco pelo nome..."
          autoComplete="off"
          className={`w-full border border-border rounded-md px-3 py-2 text-base focus:outline-none focus:ring-2 focus:ring-primary ${value ? 'pl-10' : ''} ${value ? 'pr-9' : ''}`}
        />
        {value && (
          <button
            type="button"
            onClick={clear}
            aria-label="Remover banco selecionado"
            className="absolute right-2 top-1/2 -translate-y-1/2 text-text-secondary hover:text-text-primary px-1"
          >
            ×
          </button>
        )}
      </div>

      {open && (loading || suggestions.length > 0) && (
        <div className="absolute z-10 mt-1 w-full bg-surface border border-border rounded-md shadow-soft max-h-64 overflow-y-auto">
          {loading ? (
            <p className="px-3 py-2 text-sm text-text-secondary">Buscando...</p>
          ) : (
            suggestions.map(bank => (
              <button
                type="button"
                key={bank.ispb}
                onClick={() => select(bank)}
                className="w-full text-left px-3 py-2 text-sm hover:bg-background flex items-center gap-3"
              >
                <BankLogo bank={bank} />
                <span className="truncate">{bank.name}</span>
              </button>
            ))
          )}
        </div>
      )}
      {error && <div className="text-red-600 text-sm mt-1">{error}</div>}
    </div>
  )
}
