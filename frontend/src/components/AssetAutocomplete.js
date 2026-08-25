import { useEffect, useRef, useState } from 'react'
import { assetSearchService } from '@/services/assetSearchService'

// Campo de nome com sugestão de ticker via brapi.dev (Ações/FII). Sempre permite
// digitar livre — se a busca não achar nada ou a API estiver fora, o usuário só
// continua digitando manualmente, sem travar o formulário.
export default function AssetAutocomplete({ label, value, onChange, assetType, placeholder, error }) {
  const [suggestions, setSuggestions] = useState([])
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const debounceRef = useRef(null)
  const containerRef = useRef(null)

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current)

    if (!value || value.trim().length < 2) {
      setSuggestions([])
      return undefined
    }

    debounceRef.current = setTimeout(() => {
      setLoading(true)
      assetSearchService.search({ type: assetType, search: value.trim() })
        .then(res => {
          setSuggestions(res.data)
          setOpen(true)
        })
        .catch(() => setSuggestions([]))
        .finally(() => setLoading(false))
    }, 300)

    return () => clearTimeout(debounceRef.current)
  }, [value, assetType])

  useEffect(() => {
    const handleClickOutside = (e) => {
      if (containerRef.current && !containerRef.current.contains(e.target)) setOpen(false)
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const selectSuggestion = (s) => {
    onChange(s.symbol)
    setSuggestions([])
    setOpen(false)
  }

  return (
    <div className="mb-4 relative" ref={containerRef}>
      {label && <label className="block text-sm text-text-secondary mb-1">{label}</label>}
      <input
        type="text"
        value={value}
        onChange={e => { onChange(e.target.value); setOpen(true) }}
        onFocus={() => suggestions.length > 0 && setOpen(true)}
        placeholder={placeholder}
        autoComplete="off"
        className="w-full border border-border rounded-md px-3 py-2 text-base focus:outline-none focus:ring-2 focus:ring-primary"
      />
      {open && (loading || suggestions.length > 0) && (
        <div className="absolute z-10 mt-1 w-full bg-surface border border-border rounded-md shadow-soft max-h-56 overflow-y-auto">
          {loading ? (
            <p className="px-3 py-2 text-sm text-text-secondary">Buscando...</p>
          ) : (
            suggestions.map(s => (
              <button
                type="button"
                key={s.symbol}
                onClick={() => selectSuggestion(s)}
                className="w-full text-left px-3 py-2 text-sm hover:bg-background flex justify-between items-center gap-3"
              >
                <span className="font-medium shrink-0">{s.symbol}</span>
                <span className="text-text-secondary truncate">{s.name}</span>
              </button>
            ))
          )}
        </div>
      )}
      {error && <div className="text-red-600 text-sm mt-1">{error}</div>}
    </div>
  )
}
