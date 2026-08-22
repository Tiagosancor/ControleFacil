import { api } from '@/lib/api'

export const investmentCategoryService = {
  list: (params) => api().get('/api/investment-categories', { params }),
  getById: (id) => api().get(`/api/investment-categories/${id}`),
  create: (data) => api().post('/api/investment-categories', data),
  update: (id, data) => api().put(`/api/investment-categories/${id}`, data),
  remove: (id) => api().delete(`/api/investment-categories/${id}`),
}
