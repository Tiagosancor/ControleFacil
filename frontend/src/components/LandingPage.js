import { useState } from 'react'
import Link from 'next/link'
import { AreaChart, Area, ResponsiveContainer } from 'recharts'
import Card from '@/components/ui/Card'
import Button from '@/components/ui/Button'
import FormInput from '@/components/FormInput'
import { contactService } from '@/services/contactService'

const HERO_GROWTH = [
  { value: 62000 }, { value: 65500 }, { value: 68000 },
  { value: 71000 }, { value: 76500 }, { value: 84320 },
]

function LinkButton({ href, variant = 'primary', className = '', children }) {
  const base = 'rounded-md px-5 py-2.5 text-sm font-semibold transition-colors inline-flex items-center justify-center gap-2'
  const variants = {
    primary: 'bg-primary text-white hover:bg-primary-hover',
    secondary: 'bg-transparent border border-border text-text-primary hover:bg-background',
    inverse: 'bg-white text-primary hover:bg-white/90',
  }
  return (
    <Link href={href} className={`${base} ${variants[variant]} ${className}`}>
      {children}
    </Link>
  )
}

function ChatIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2Z" />
      <line x1="8" y1="9" x2="16" y2="9" />
      <line x1="8" y1="13" x2="13" y2="13" />
    </svg>
  )
}

function LayersIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <polygon points="12 2 2 7 12 12 22 7 12 2" />
      <polyline points="2 17 12 22 22 17" />
      <polyline points="2 12 12 17 22 12" />
    </svg>
  )
}

function ShieldIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10Z" />
    </svg>
  )
}

function DashboardIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <line x1="18" y1="20" x2="18" y2="10" />
      <line x1="12" y1="20" x2="12" y2="4" />
      <line x1="6" y1="20" x2="6" y2="14" />
    </svg>
  )
}

function ZapIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2" />
    </svg>
  )
}

function TargetIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <circle cx="12" cy="12" r="9" />
      <circle cx="12" cy="12" r="5" />
      <circle cx="12" cy="12" r="1" />
    </svg>
  )
}

function TrendingUpIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <polyline points="23 6 13.5 15.5 8.5 10.5 1 18" />
      <polyline points="17 6 23 6 23 12" />
    </svg>
  )
}

function CheckIcon(props) {
  return (
    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round" {...props}>
      <polyline points="20 6 9 17 4 12" />
    </svg>
  )
}

const WHY_ITEMS = [
  { Icon: ChatIcon, title: 'Sem jargão financeiro', text: 'Explicações simples, sem economês. Você entende exatamente pra onde vai o seu dinheiro.' },
  { Icon: LayersIcon, title: 'Controle e investimento juntos', text: 'Suas contas do dia a dia e o seu patrimônio investido, no mesmo painel — sem precisar de duas ferramentas.' },
  { Icon: ShieldIcon, title: 'Segurança de verdade', text: 'Autenticação segura e dados protegidos. Suas informações financeiras são só suas.' },
]

const FEATURES = [
  { Icon: DashboardIcon, title: 'Painel geral', text: 'Receitas, despesas e patrimônio total resumidos visualmente, sempre atualizados.' },
  { Icon: ZapIcon, title: 'Lançamentos rápidos', text: 'Registre uma receita ou despesa em poucos toques, de qualquer tela do app.' },
  { Icon: TargetIcon, title: 'Metas e orçamento', text: 'Defina limites por categoria e metas mensais, com progresso visual acompanhando você.' },
  { Icon: TrendingUpIcon, title: 'Investimentos', text: 'Acompanhe renda fixa, ações, fundos e mais — tudo somado ao seu patrimônio total.' },
]

const PLANS = [
  {
    key: 'free',
    name: 'Free',
    highlight: false,
    monthly: 0,
    annual: 0,
    priceLabel: () => 'R$0',
    priceSuffix: () => '',
    cta: 'Criar conta grátis',
    features: ['Até 20 lançamentos por mês', 'Até 2 contas bancárias', 'Categorias padrão', 'App mobile'],
  },
  {
    key: 'premium',
    name: 'Premium',
    highlight: true,
    monthly: 14.9,
    annual: 99.9,
    priceLabel: (billing) => billing === 'monthly' ? 'R$14,90' : 'R$99,90',
    priceSuffix: (billing) => billing === 'monthly' ? '/mês' : '/ano',
    cta: 'Assinar Premium',
    features: ['Tudo do Free, ilimitado', 'Metas mensais e orçamento por categoria', 'Alertas de vencimento', 'Cartões de crédito', 'Dashboard completo'],
  },
  {
    key: 'pro',
    name: 'Pro',
    highlight: false,
    monthly: 19.9,
    annual: 149.9,
    priceLabel: (billing) => billing === 'monthly' ? 'R$19,90' : 'R$149,90',
    priceSuffix: (billing) => billing === 'monthly' ? '/mês' : '/ano',
    cta: 'Assinar Pro',
    features: ['Tudo do Premium', 'Acompanhamento de investimentos', 'Calculadora financeira', 'Relatórios completos', 'Metas de longo prazo'],
  },
]

function Nav() {
  return (
    <header className="sticky top-0 z-40 bg-surface/95 backdrop-blur border-b border-border">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 h-16 flex items-center justify-between">
        <Link href="/" className="flex items-center gap-2 font-heading font-semibold text-lg text-primary">
          <img src="/favicon.png" alt="" className="h-7 w-7" />
          Semeia Grana
        </Link>
        <div className="flex items-center gap-2 sm:gap-4">
          <Link href="/login" className="text-sm text-text-secondary hover:text-text-primary px-2">Entrar</Link>
          <LinkButton href="/register">Criar conta</LinkButton>
        </div>
      </div>
    </header>
  )
}

function Hero() {
  return (
    <section className="max-w-6xl mx-auto px-4 sm:px-6 py-14 sm:py-24 grid grid-cols-1 lg:grid-cols-2 gap-10 lg:gap-16 items-center">
      <div>
        <span className="inline-block bg-gold-wash text-gold text-xs font-semibold rounded-full px-3 py-1 mb-5">
          Controle do dia a dia + investimentos, em um só lugar
        </span>
        <h1 className="font-heading text-4xl sm:text-5xl font-semibold leading-tight text-text-primary mb-5">
          Cuide do seu dinheiro <span className="text-primary">sem complicação</span>, mesmo sem entender de finanças.
        </h1>
        <p className="text-lg text-text-secondary mb-8 max-w-lg">
          O Semeia Grana organiza seus lançamentos, contas, metas e investimentos automaticamente — em linguagem simples, sem economês.
        </p>
        <div className="flex flex-wrap gap-3">
          <LinkButton href="/register">Criar conta grátis</LinkButton>
          <LinkButton href="/login" variant="secondary">Já tenho conta</LinkButton>
        </div>
      </div>

      <Card className="p-6">
        <p className="text-sm text-text-secondary mb-1">Patrimônio Total</p>
        <p className="font-heading text-3xl font-semibold text-primary mb-1">R$ 84.320,50</p>
        <p className="text-sm text-gold font-semibold mb-4">▲ 12,4% nos últimos 6 meses</p>
        <ResponsiveContainer width="100%" height={140}>
          <AreaChart data={HERO_GROWTH}>
            <defs>
              <linearGradient id="heroGrowth" x1="0" y1="0" x2="0" y2="1">
                <stop offset="0%" stopColor="#285649" stopOpacity={0.35} />
                <stop offset="100%" stopColor="#285649" stopOpacity={0} />
              </linearGradient>
            </defs>
            <Area type="monotone" dataKey="value" stroke="#285649" strokeWidth={2} fill="url(#heroGrowth)" isAnimationActive={false} />
          </AreaChart>
        </ResponsiveContainer>
        <p className="text-xs text-text-muted mt-2">Exemplo ilustrativo</p>
      </Card>
    </section>
  )
}

function WhySection() {
  return (
    <section className="bg-panel py-16 sm:py-20">
      <div className="max-w-6xl mx-auto px-4 sm:px-6">
        <h2 className="font-heading text-2xl sm:text-3xl font-semibold text-center mb-12">Por que existimos</h2>
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-8">
          {WHY_ITEMS.map(({ Icon, title, text }) => (
            <div key={title} className="text-center px-2">
              <div className="h-12 w-12 rounded-full bg-primary-soft text-primary flex items-center justify-center mx-auto mb-4">
                <Icon className="h-6 w-6" />
              </div>
              <h3 className="font-heading font-semibold text-lg mb-2">{title}</h3>
              <p className="text-text-secondary text-sm">{text}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}

function FeaturesSection() {
  return (
    <section className="py-16 sm:py-20">
      <div className="max-w-6xl mx-auto px-4 sm:px-6">
        <h2 className="font-heading text-2xl sm:text-3xl font-semibold text-center mb-12">
          Tudo o que você precisa pra organizar sua vida financeira
        </h2>
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          {FEATURES.map(({ Icon, title, text }) => (
            <Card key={title} className="h-full">
              <div className="h-12 w-12 rounded-full bg-primary-soft text-primary flex items-center justify-center mb-4">
                <Icon className="h-6 w-6" />
              </div>
              <h3 className="font-heading font-semibold text-lg mb-2">{title}</h3>
              <p className="text-text-secondary text-sm">{text}</p>
            </Card>
          ))}
        </div>
      </div>
    </section>
  )
}

function PricingSection() {
  const [billing, setBilling] = useState('monthly')

  return (
    <section className="bg-panel py-16 sm:py-20" id="planos">
      <div className="max-w-6xl mx-auto px-4 sm:px-6">
        <h2 className="font-heading text-2xl sm:text-3xl font-semibold text-center mb-2">Planos para cada momento</h2>
        <p className="text-text-secondary text-center mb-10">Comece grátis. Evolua quando fizer sentido pra você.</p>

        <div className="flex justify-center mb-10">
          <div className="inline-flex bg-surface border border-border rounded-full p-1">
            <button
              onClick={() => setBilling('monthly')}
              className={`px-4 py-1.5 rounded-full text-sm font-medium transition-colors ${billing === 'monthly' ? 'bg-primary text-white' : 'text-text-secondary'}`}
            >
              Mensal
            </button>
            <button
              onClick={() => setBilling('annual')}
              className={`px-4 py-1.5 rounded-full text-sm font-medium transition-colors ${billing === 'annual' ? 'bg-primary text-white' : 'text-text-secondary'}`}
            >
              Anual
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6 items-start">
          {PLANS.map(plan => (
            <Card
              key={plan.key}
              className={`relative h-full flex flex-col ${plan.highlight ? 'border-2 border-primary shadow-md' : ''}`}
            >
              {plan.highlight && (
                <span className="absolute -top-3 left-1/2 -translate-x-1/2 bg-primary text-white text-xs font-semibold px-3 py-1 rounded-full">
                  Mais popular
                </span>
              )}
              <h3 className="font-heading text-xl font-semibold mb-1">{plan.name}</h3>
              <p className="mb-6">
                <span className="font-heading text-3xl font-semibold text-text-primary">{plan.priceLabel(billing)}</span>
                <span className="text-text-secondary text-sm">{plan.priceSuffix(billing)}</span>
              </p>
              <ul className="space-y-2.5 mb-8 flex-1">
                {plan.features.map(feature => (
                  <li key={feature} className="flex items-start gap-2 text-sm text-text-secondary">
                    <CheckIcon className="h-4 w-4 text-primary shrink-0 mt-0.5" />
                    {feature}
                  </li>
                ))}
              </ul>
              <LinkButton href="/register" variant={plan.highlight ? 'primary' : 'secondary'} className="w-full">
                {plan.cta}
              </LinkButton>
            </Card>
          ))}
        </div>
      </div>
    </section>
  )
}

function ContactSection() {
  const [name, setName] = useState('')
  const [email, setEmail] = useState('')
  const [message, setMessage] = useState('')
  const [errors, setErrors] = useState({})
  const [submitting, setSubmitting] = useState(false)
  const [sent, setSent] = useState(false)

  const submit = async (ev) => {
    ev.preventDefault()
    const errs = {}
    if (!name) errs.name = 'Nome é obrigatório'
    if (!email) errs.email = 'Email é obrigatório'
    if (!message) errs.message = 'Mensagem é obrigatória'
    setErrors(errs)
    if (Object.keys(errs).length) return
    setSubmitting(true)
    try {
      await contactService.send({ name, email, message })
      setSent(true)
      setName('')
      setEmail('')
      setMessage('')
    } catch (err) {
      const status = err?.response?.status
      setErrors({
        form: err?.response?.data?.error
          || (status === 429 ? 'Muitas tentativas. Tente novamente em instantes.' : 'Não foi possível enviar sua mensagem agora. Tente novamente mais tarde.'),
      })
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <section className="py-16 sm:py-20" id="contato">
      <div className="max-w-xl mx-auto px-4 sm:px-6">
        <h2 className="font-heading text-2xl sm:text-3xl font-semibold text-center mb-2">Fale conosco</h2>
        <p className="text-text-secondary text-center mb-10">Dúvidas, sugestões ou parcerias — é só mandar uma mensagem.</p>
        <Card>
          {sent ? (
            <div className="text-center py-6">
              <p className="font-heading font-semibold text-lg text-primary mb-2">Mensagem enviada!</p>
              <p className="text-text-secondary text-sm">Retornaremos o quanto antes.</p>
            </div>
          ) : (
            <form onSubmit={submit}>
              <FormInput label="Nome" value={name} onChange={setName} error={errors.name} />
              <FormInput label="Email" value={email} onChange={setEmail} error={errors.email} />
              <FormInput label="Mensagem" textarea rows={4} value={message} onChange={setMessage} error={errors.message} />
              {errors.form && <div className="text-red-600 text-sm mt-1 mb-2">{errors.form}</div>}
              <Button variant="primary" type="submit" className="w-full mt-2" loading={submitting}>
                {submitting ? 'Enviando...' : 'Enviar mensagem'}
              </Button>
            </form>
          )}
        </Card>
      </div>
    </section>
  )
}

function FinalCta() {
  return (
    <section className="bg-primary py-16 sm:py-20">
      <div className="max-w-3xl mx-auto px-4 sm:px-6 text-center">
        <h2 className="font-heading text-2xl sm:text-3xl font-semibold text-white mb-4">
          Pronto pra organizar sua vida financeira?
        </h2>
        <p className="text-white/80 mb-8">Comece grátis, sem cartão de crédito.</p>
        <LinkButton href="/register" variant="inverse">Criar conta grátis</LinkButton>
      </div>
    </section>
  )
}

function Footer() {
  return (
    <footer className="py-10 bg-surface border-t border-border">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 flex flex-col sm:flex-row items-center justify-between gap-4">
        <div className="flex items-center gap-2">
          <img src="/favicon.png" alt="" className="h-6 w-6" />
          <span className="font-heading font-semibold text-text-primary">Semeia Grana</span>
        </div>
        <p className="text-sm text-text-secondary">© {new Date().getFullYear()} Semeia Grana. Todos os direitos reservados.</p>
      </div>
    </footer>
  )
}

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-background text-text-primary">
      <Nav />
      <Hero />
      <WhySection />
      <FeaturesSection />
      <PricingSection />
      <ContactSection />
      <FinalCta />
      <Footer />
    </div>
  )
}
