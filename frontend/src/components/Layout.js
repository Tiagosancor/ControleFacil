export default function Layout({ children }) {
  return (
    <div className="min-h-screen flex items-center justify-center px-4">
      <div className="w-full max-w-sm">
        <div className="text-center mb-6">
          <span className="text-lg font-semibold text-accent">ControleFacil</span>
        </div>
        {children}
      </div>
    </div>
  )
}
