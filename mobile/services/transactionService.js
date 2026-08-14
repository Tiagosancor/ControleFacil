import { api } from './api';

export const transactionService = {
  list: (params) => api.get('/api/transactions', { params }),
  getById: (id) => api.get(`/api/transactions/${id}`),
  create: (data) => api.post('/api/transactions', data),
  update: (id, data) => api.put(`/api/transactions/${id}`, data),
  remove: (id) => api.delete(`/api/transactions/${id}`),
  removeSeries: (seriesId) => api.delete(`/api/transactions/series/${seriesId}`),
};
