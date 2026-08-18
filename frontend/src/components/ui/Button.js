import Spinner from './Spinner'

export default function Button({ variant = 'secondary', className = '', loading = false, disabled, children, ...props }) {
  const base = 'rounded-md px-4 py-2 text-sm font-medium transition-colors inline-flex items-center justify-center gap-2 disabled:opacity-60 disabled:cursor-not-allowed'
  const variants = {
    primary: 'bg-primary text-white hover:bg-primary-hover',
    secondary: 'bg-transparent border border-border text-text-primary hover:bg-background',
    danger: 'bg-red-600 text-white hover:bg-red-700',
  }
  return (
    <button className={`${base} ${variants[variant]} ${className}`} disabled={disabled || loading} {...props}>
      {loading && <Spinner className="h-4 w-4" />}
      {children}
    </button>
  )
}
