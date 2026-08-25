import { api } from './api';

export const bankService = {
  search: (params) => api.get('/api/banks', { params }),
};
