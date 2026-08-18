import { useState } from 'react'

function EyeIcon({ open, className }) {
  return open ? (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={className}>
      <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8Z" />
      <circle cx="12" cy="12" r="3" />
    </svg>
  ) : (
    <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" className={className}>
      <path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24" />
      <path d="M6.61 6.61A18.5 18.5 0 0 0 1 12s4 8 11 8a9.26 9.26 0 0 0 5.39-1.61" />
      <line x1="1" y1="1" x2="23" y2="23" />
    </svg>
  )
}

export default function FormInput({ label, value, onChange, error, textarea, type, ...props }) {
  const [showPassword, setShowPassword] = useState(false)
  const isPassword = type === 'password'
  const inputType = isPassword && showPassword ? 'text' : type

  return (
    <div className="mb-4">
      {label && <label className="block text-sm text-text-secondary mb-1">{label}</label>}
      {textarea ? (
        <textarea
          value={value}
          onChange={e => onChange(e.target.value)}
          {...props}
          className="w-full border border-border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
      ) : isPassword ? (
        <div className="relative">
          <input
            type={inputType}
            value={value}
            onChange={e => onChange(e.target.value)}
            {...props}
            className="w-full border border-border rounded-md px-3 py-2 pr-10 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
          />
          <button
            type="button"
            onClick={() => setShowPassword(s => !s)}
            className="absolute inset-y-0 right-0 flex items-center px-3 text-text-secondary hover:text-text-primary"
            aria-label={showPassword ? 'Ocultar senha' : 'Mostrar senha'}
            tabIndex={-1}
          >
            <EyeIcon open={showPassword} className="h-4 w-4" />
          </button>
        </div>
      ) : (
        <input
          type={type}
          value={value}
          onChange={e => onChange(e.target.value)}
          {...props}
          className="w-full border border-border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary"
        />
      )}
      {error && <div className="text-red-600 text-sm mt-1">{error}</div>}
    </div>
  )
}
