import { api } from './api';

export const bankAccountService = {
  list: (params) => api.get('/api/bank-accounts', { params }),
  getById: (id) => api.get(`/api/bank-accounts/${id}`),
  create: (data) => api.post('/api/bank-accounts', data),
  update: (id, data) => api.put(`/api/bank-accounts/${id}`, data),
  remove: (id) => api.delete(`/api/bank-accounts/${id}`),
};
