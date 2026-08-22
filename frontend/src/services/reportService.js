import { api } from '@/lib/api'

export const reportService = {
  getDre: (params) => api().get('/api/reports/dre', { params }),
  getPending: () => api().get('/api/reports/pending'),
}
