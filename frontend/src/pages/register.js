import { useState } from 'react'
import Link from 'next/link'
import Router from 'next/router'
import { authService } from '@/services/authService'
import FormInput from '@/components/FormInput'
import Layout from '@/components/Layout'
import Button from '@/components/ui/Button'
import Card from '@/components/ui/Card'

export default function Register() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)

  const submit = async (ev) => {
    ev.preventDefault()
    const errs = {}
    if (!name) errs.name = 'Nome é obrigatório'
    if (!email) errs.email = 'Email é obrigatório'
    if (!password) errs.password = 'Senha é obrigatória'
    if (password && password.length < 8) errs.password = 'Senha deve ter ao menos 8 caracteres'
    setErrors(errs)
    if (Object.keys(errs).length) return
    setSubmitting(true)
    try {
      await authService.register({ name, email, password })
      Router.push('/login')
    } catch (err) {
      const apiErrors = err?.response?.data?.errors
      setErrors({
        form: err?.response?.data?.error
          || (apiErrors && Object.values(apiErrors).flat().join(' '))
          || 'Registro falhou',
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Layout>
      <Card>
        <h1 className="text-2xl font-semibold mb-6">Criar conta</h1>
        <form onSubmit={submit}>
          <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />
          <FormInput label="Email" value={email} onChange={setEmail} error={errors.email} />
          <FormInput label="Senha" type="password" value={password} onChange={setPassword} error={errors.password} />
          {errors.form && <div className="text-red-600 text-sm mt-1 mb-2">{errors.form}</div>}
          <Button variant="primary" type="submit" className="w-full mt-2" loading={submitting}>
            {submitting ? 'Registrando...' : 'Registrar'}
          </Button>
        </form>
        <p className="text-sm text-text-secondary mt-4 text-center">
          Já tem conta? <Link href="/login" className="text-accent">Entrar</Link>
        </p>
      </Card>
    </Layout>
  )
}
