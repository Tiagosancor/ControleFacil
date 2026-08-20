export default function Card({ children, className = '' }) {
  return (
    <div className={`bg-surface border border-border rounded-xl shadow-soft p-5 ${className}`}>
      {children}
    </div>
  )
}
