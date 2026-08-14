export default function Input({ className = '', ...props }) {
  return (
    <input
      {...props}
      className={`w-full border border-border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent ${className}`}
    />
  )
}
