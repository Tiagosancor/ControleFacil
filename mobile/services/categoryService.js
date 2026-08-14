import { api } from './api';

export const categoryService = {
  list: (params) => api.get('/api/categories', { params }),
  getById: (id) => api.get(`/api/categories/${id}`),
  create: (data) => api.post('/api/categories', data),
  update: (id, data) => api.put(`/api/categories/${id}`, data),
  remove: (id) => api.delete(`/api/categories/${id}`),
};
