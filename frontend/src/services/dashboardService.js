import { api } from '@/lib/api'

export const dashboardService = {
  getMonthlySummary: (params) => api().get('/api/dashboard/summary', { params }),
  getDueSoon: () => api().get('/api/dashboard/due-soon'),
  getGoalComparison: (params) => api().get('/api/dashboard/goal-comparison', { params }),
  getHistorical: (params) => api().get('/api/dashboard/historical', { params }),
  getAnalyses: (params) => api().get('/api/dashboard/analyses', { params }),
}
