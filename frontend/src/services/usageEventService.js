import { api } from '@/lib/api'

export const usageEventService = {
  getLoginHistory: (params) => api().get('/api/usage-events/login-history', { params }),
  getLoggedInUsers: (params) => api().get('/api/usage-events/logged-in-users', { params }),
}
