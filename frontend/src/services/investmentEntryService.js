import { api } from '@/lib/api'

export const investmentEntryService = {
  list: (params) => api().get('/api/investment-entries', { params }),
  getById: (id) => api().get(`/api/investment-entries/${id}`),
  create: (data) => api().post('/api/investment-entries', data),
  update: (id, data) => api().put(`/api/investment-entries/${id}`, data),
  remove: (id) => api().delete(`/api/investment-entries/${id}`),
}
