import axios from 'axios'

const API_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000'

export function api() {
  let token = null
  if (typeof window !== 'undefined') token = localStorage.getItem('token')
  const headers = token ? { Authorization: `Bearer ${token}` } : {}
  const instance = axios.create({ baseURL: API_URL, headers })
  instance.interceptors.response.use(
    response => response,
    error => {
      if (error.response?.status === 401 && error.config.url !== '/api/auth/login') {
        localStorage.removeItem('token')
        window.location.href = '/login'
      }
      return Promise.reject(error)
    }
  )
  return instance
}

export default api
