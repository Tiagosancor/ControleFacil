import { api } from '@/lib/api'

export const categoryBudgetService = {
  list: (params) => api().get('/api/category-budgets', { params }),
  getById: (id) => api().get(`/api/category-budgets/${id}`),
  create: (data) => api().post('/api/category-budgets', data),
  update: (id, data) => api().put(`/api/category-budgets/${id}`, data),
  remove: (id) => api().delete(`/api/category-budgets/${id}`),
}
