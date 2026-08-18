export default function Select({ className = '', children, ...props }) {
  return (
    <select
      {...props}
      className={`w-full border border-border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary ${className}`}
    >
      {children}
    </select>
  )
}
