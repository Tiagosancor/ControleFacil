import { useState } from 'react'
import Link from 'next/link'
import { authService } from '@/services/authService'
import FormInput from '@/components/FormInput'
import Layout from '@/components/Layout'
import { useAuth } from '@/contexts/AuthContext'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function Login() {
  const { login } = useAuth()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const submit = async (ev) => {
    ev.preventDefault()
    const errs = {}
    if (!email) errs.email = 'Email é obrigatório'
    if (!password) errs.password = 'Senha é obrigatória'
    setErrors(errs)
    if (Object.keys(errs).length) return
    setSubmitting(true)
    try {
      const res = await authService.login({ email, password })
      login(res.data.token)
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Login falhou' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Layout>
      <Card>
        <h1 className="text-2xl font-heading font-semibold mb-6">Entrar</h1>
        <form onSubmit={submit}>
          <FormInput label="Email" value={email} onChange={setEmail} error={errors.email} />
          <FormInput label="Senha" type="password" value={password} onChange={setPassword} error={errors.password} />
          <div className="text-right -mt-2 mb-2">
            <Link href="/forgot-password" className="text-sm text-link">Esqueci minha senha</Link>
          </div>
          {errors.form && <div className="text-red-600 text-sm mt-1 mb-2">{errors.form}</div>}
          <Button variant="primary" type="submit" className="w-full mt-2" loading={submitting}>
            {submitting ? 'Entrando...' : 'Entrar'}
          </Button>
        </form>
        <p className="text-sm text-text-secondary mt-4 text-center">
          Não tem conta? <Link href="/register" className="text-link">Criar conta</Link>
        </p>
      </Card>
    </Layout>
  )
}
