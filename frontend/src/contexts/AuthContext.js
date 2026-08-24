import { createContext, useContext, useEffect, useState } from 'react'
import { useRouter } from 'next/router'
import { authService } from '@/services/authService'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const router = useRouter()
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null
    if (!token) {
      setLoading(false)
    } else {
      authService.getCurrentUser()
        .then(res => {
          setUser(res.data)
          setLoading(false)
        })
        .catch(() => {
          setLoading(false)
        })
    }
  }, [])

  const login = (token) => {
    localStorage.setItem('token', token)
    authService.getCurrentUser()
      .then(res => {
        setUser(res.data)
        router.push('/')
      })
  }

  const logout = () => {
    localStorage.removeItem('token')
    setUser(null)
    router.push('/login')
  }

  const isAdmin = user?.role === 'Admin'

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, isAdmin }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
