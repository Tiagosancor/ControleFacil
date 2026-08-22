import { api } from '@/lib/api'

export const monthlyGoalService = {
  list: (params) => api().get('/api/monthly-goals', { params }),
  getById: (id) => api().get(`/api/monthly-goals/${id}`),
  create: (data) => api().post('/api/monthly-goals', data),
  update: (id, data) => api().put(`/api/monthly-goals/${id}`, data),
  remove: (id) => api().delete(`/api/monthly-goals/${id}`),
}
