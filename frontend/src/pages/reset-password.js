import { useState } from 'react'
import Link from 'next/link'
import Router, { useRouter } from 'next/router'
import { authService } from '@/services/authService'
import FormInput from '@/components/FormInput'
import Layout from '@/components/Layout'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function ResetPassword() {
  const router = useRouter()
  const { token } = router.query
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)
  const [success, setSuccess] = useState(false)

  const submit = async (ev) => {
    ev.preventDefault()
    const errs = {}
    if (!password) errs.password = 'Senha é obrigatória'
    else if (password.length < 8) errs.password = 'Senha deve ter ao menos 8 caracteres'
    if (confirmPassword !== password) errs.confirmPassword = 'As senhas não conferem'
    setErrors(errs)
    if (Object.keys(errs).length) return
    setSubmitting(true)
    try {
      await authService.resetPassword({ token, newPassword: password })
      setSuccess(true)
      setTimeout(() => Router.push('/login'), 2000)
    } catch (err) {
      setErrors({ form: err?.response?.data?.error || 'Não foi possível redefinir a senha. O link pode ter expirado.' })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Layout>
      <Card>
        <h1 className="text-2xl font-semibold mb-6">Redefinir senha</h1>
        {!router.isReady ? null : !token ? (
          <p className="text-sm text-red-600">Link inválido. Solicite uma nova recuperação de senha.</p>
        ) : success ? (
          <p className="text-sm text-text-secondary">Senha redefinida com sucesso! Redirecionando para o login...</p>
        ) : (
          <form onSubmit={submit}>
            <FormInput label="Nova senha" type="password" value={password} onChange={setPassword} error={errors.password} />
            <FormInput label="Confirmar nova senha" type="password" value={confirmPassword} onChange={setConfirmPassword} error={errors.confirmPassword} />
            {errors.form && <div className="text-red-600 text-sm mt-1 mb-2">{errors.form}</div>}
            <Button variant="primary" type="submit" className="w-full mt-2" loading={submitting}>
              {submitting ? 'Redefinindo...' : 'Redefinir senha'}
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
