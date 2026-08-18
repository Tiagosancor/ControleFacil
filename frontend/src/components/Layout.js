export default function Layout({ children }) {
  return (
    <div className="min-h-screen flex items-center justify-center px-4">
      <div className="w-full max-w-sm">
        <div className="text-center mb-6">
          <img
            src="/logo-semeiagrana.png"
            alt="Semeia Grana — controle e investimento simples"
            className="mx-auto w-72 h-auto max-w-[85%]"
          />
        </div>
        {children}
      </div>
    </div>
  )
}
