import { api } from '@/lib/api'

export const creditCardService = {
  list: (params) => api().get('/api/credit-cards', { params }),
  getById: (id) => api().get(`/api/credit-cards/${id}`),
  create: (data) => api().post('/api/credit-cards', data),
  update: (id, data) => api().put(`/api/credit-cards/${id}`, data),
  remove: (id) => api().delete(`/api/credit-cards/${id}`),
  getInvoice: (id, params) => api().get(`/api/credit-cards/${id}/invoice`, { params }),
}
