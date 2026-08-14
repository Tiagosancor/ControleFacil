export default function Button({ variant = 'secondary', className = '', ...props }) {
  const base = 'rounded-md px-4 py-2 text-sm font-medium transition-colors'
  const variants = {
    primary: 'bg-accent text-white hover:bg-accent-hover',
    secondary: 'bg-transparent border border-border text-text-primary hover:bg-background',
    danger: 'bg-red-600 text-white hover:bg-red-700',
  }
  return <button className={`${base} ${variants[variant]} ${className}`} {...props} />
}
