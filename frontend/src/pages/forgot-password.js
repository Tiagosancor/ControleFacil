import { useState } from 'react'
import Link from 'next/link'
import { authService } from '@/services/authService'
import FormInput from '@/components/FormInput'
import Layout from '@/components/Layout'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function ForgotPassword() {
  const [email, setEmail] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)
  const [sent, setSent] = useState(false)

  const submit = async (ev) => {
    ev.preventDefault()
    if (!email) {
      setErrors({ email: 'Email é obrigatório' })
      return
    }
    setErrors({})
    setSubmitting(true)
    try {
      await authService.forgotPassword({ email })
      setSent(true)
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Não foi possível enviar o e-mail de recuperação' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Layout>
      <Card>
        <h1 className="text-2xl font-heading font-semibold mb-6">Esqueci minha senha</h1>
        {sent ? (
          <p className="text-sm text-text-secondary">
            Se o e-mail informado estiver cadastrado, você receberá um link de recuperação em instantes. Verifique também a caixa de spam.
          </p>
        ) : (
          <form onSubmit={submit}>
            <p className="text-sm text-text-secondary mb-4">
              Informe o e-mail da sua conta para receber um link de redefinição de senha.
            </p>
            <FormInput label="Email" type="email" value={email} onChange={setEmail} error={errors.email} />
            {errors.form && <div className="text-red-600 text-sm mt-1 mb-2">{errors.form}</div>}
            <Button variant="primary" type="submit" className="w-full mt-2" loading={submitting}>
              {submitting ? 'Enviando...' : 'Enviar link de recuperação'}
            </Button>
          </form>
        )}
        <p className="text-sm text-text-secondary mt-4 text-center">
          <Link href="/login" className="text-link">Voltar para o login</Link>
        </p>
      </Card>
    </Layout>
  )
}
