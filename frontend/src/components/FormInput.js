export default function FormInput({ label, value, onChange, error, textarea, type, ...props }) {
  return (
    <div className="mb-4">
      {label && <label className="block text-sm text-text-secondary mb-1">{label}</label>}
      {textarea ? (
        <textarea
          value={value}
          onChange={e => onChange(e.target.value)}
          {...props}
          className="w-full border border-border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent"
        />
      ) : (
        <input
          type={type}
          value={value}
          onChange={e => onChange(e.target.value)}
          {...props}
          className="w-full border border-border rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent"
        />
      )}
      {error && <div className="text-red-600 text-sm mt-1">{error}</div>}
    </div>
  )
}
