import { useEffect, useState } from 'react'
import Head from 'next/head'
import Router from 'next/router'
import '../styles/globals.css'
import { AuthProvider } from '../contexts/AuthContext'

// Barra de progresso indeterminada no topo durante troca de rota: entra em
// transição longa (8s) rumo a 85% enquanto a navegação está em andamento — se
// a página carregar rápido, ela mal se move; se demorar (cold start do Render
// em produção, por exemplo), ela segue avançando devagar em vez de travar.
// Ao terminar, completa pra 100% rapidinho e desaparece.
function TopProgressBar() {
  const [state, setState] = useState('idle') // idle | loading | done

  useEffect(() => {
    let hideTimeout
    const start = () => {
      clearTimeout(hideTimeout)
      setState('loading')
    }
    const done = () => {
      setState('done')
      hideTimeout = setTimeout(() => setState('idle'), 200)
    }
    Router.events.on('routeChangeStart', start)
    Router.events.on('routeChangeComplete', done)
    Router.events.on('routeChangeError', done)
    return () => {
      Router.events.off('routeChangeStart', start)
      Router.events.off('routeChangeComplete', done)
      Router.events.off('routeChangeError', done)
      clearTimeout(hideTimeout)
    }
  }, [])

  if (state === 'idle') return null

  return (
    <div className="fixed top-0 left-0 right-0 z-[60] h-1" aria-hidden="true">
      <div
        className={`h-full bg-primary ease-out ${
          state === 'loading' ? 'w-[85%] transition-all duration-[8000ms]' : 'w-full opacity-0 transition-all duration-200'
        }`}
      />
    </div>
  )
}

export default function App({ Component, pageProps }) {
  return (
    <AuthProvider>
      <Head>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
      </Head>
      <TopProgressBar />
      <Component {...pageProps} />
    </AuthProvider>
  )
}
