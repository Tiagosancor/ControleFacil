import { api } from '@/lib/api'

export const longTermGoalService = {
  list: () => api().get('/api/long-term-goals'),
  getById: (id) => api().get(`/api/long-term-goals/${id}`),
  create: (data) => api().post('/api/long-term-goals', data),
  update: (id, data) => api().put(`/api/long-term-goals/${id}`, data),
  remove: (id) => api().delete(`/api/long-term-goals/${id}`),
}
