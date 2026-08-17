import { api } from '@/lib/api'

export const dashboardService = {
  getMonthlySummary: (params) => api().get('/api/dashboard/summary', { params }),
}
