import { useState } from 'react'
import Link from 'next/link'
import { getCategoryIcon, getCategoryColor } from '@/lib/categoryIcons'

function categoryLabel(c) {
  return c.parentCategoryName ? `${c.parentCategoryName} > ${c.name}` : c.name
}

function CategoryCircle({ category, className = 'h-9 w-9' }) {
  const Icon = getCategoryIcon(category?.iconKey)
  const color = getCategoryColor(category?.color)
  return (
    <span className={`${className} rounded-full flex items-center justify-center shrink-0`} style={{ backgroundColor: color }}>
      <Icon className="h-1/2 w-1/2 text-white" />
    </span>
  )
}

// Seletor de categoria no padrão "círculo colorido + ícone + nome + rádio" — substitui o
// <select> nativo nas telas de Lançamentos, cobrindo categorias de sistema e próprias do
// usuário na mesma lista, com as ações fixas de gerenciamento ao final.
export default function CategoryPicker({ label = 'Categoria', categories, value, onChange, error }) {
  const [open, setOpen] = useState(false)
  const selected = categories.find(c => String(c.id) === String(value))

  const select = (category) => {
    onChange(String(category.id))
    setOpen(false)
  }

  return (
    <div className="mb-4">
      {label && <label className="block text-sm text-text-secondary mb-1">{label}</label>}

      <button
        type="button"
        onClick={() => setOpen(true)}
        className={`w-full flex items-center gap-3 border rounded-md px-3 py-2 text-left bg-surface ${error ? 'border-red-500' : 'border-border'}`}
      >
        {selected ? (
          <>
            <CategoryCircle category={selected} className="h-7 w-7" />
            <span className="flex-1 min-w-0 truncate text-sm text-text-primary">{categoryLabel(selected)}</span>
          </>
        ) : (
          <span className="text-sm text-text-secondary">Selecione uma categoria</span>
        )}
      </button>
      {error && <p className="text-red-600 text-sm mt-1">{error}</p>}

      {open && (
        <div className="fixed inset-0 z-50 flex items-end sm:items-center justify-center">
          <div className="fixed inset-0 bg-black/40" onClick={() => setOpen(false)} aria-hidden="true" />
          <div className="relative bg-surface rounded-t-xl sm:rounded-xl shadow-soft w-full sm:max-w-md max-h-[85vh] flex flex-col">
            <div className="flex justify-between items-center px-5 pt-5 pb-3">
              <h2 className="text-lg font-heading font-semibold">Categoria</h2>
              <button onClick={() => setOpen(false)} aria-label="Fechar" className="text-text-secondary hover:text-text-primary text-xl leading-none">×</button>
            </div>

            <div className="flex-1 overflow-y-auto px-2 pb-2">
              {categories.map(category => (
                <button
                  key={category.id}
                  type="button"
                  onClick={() => select(category)}
                  className="w-full flex items-center gap-3 px-3 py-2 rounded-md hover:bg-background text-left"
                >
                  <CategoryCircle category={category} />
                  <span className="flex-1 min-w-0 truncate text-sm text-text-primary">{categoryLabel(category)}</span>
                  <span className={`h-4 w-4 rounded-full border-2 shrink-0 ${String(category.id) === String(value) ? 'border-primary bg-primary' : 'border-border'}`} />
                </button>
              ))}
              {!categories.length && (
                <p className="text-sm text-text-secondary px-3 py-4">Nenhuma categoria cadastrada ainda.</p>
              )}
            </div>

            <div className="border-t border-border px-2 py-2 flex flex-col">
              <Link href="/categories/new" className="px-3 py-2 rounded-md hover:bg-background text-sm text-link" onClick={() => setOpen(false)}>
                Criar categoria
              </Link>
              <Link href="/categories/new" className="px-3 py-2 rounded-md hover:bg-background text-sm text-link" onClick={() => setOpen(false)}>
                Criar subcategoria
              </Link>
              <Link href="/categories" className="px-3 py-2 rounded-md hover:bg-background text-sm text-text-secondary" onClick={() => setOpen(false)}>
                Gerenciar categorias
              </Link>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export { categoryLabel, CategoryCircle }
